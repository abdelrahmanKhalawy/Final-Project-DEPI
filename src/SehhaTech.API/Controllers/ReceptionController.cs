using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehhaTech.Infrastructure.Data;
using SehhaTech.Core.Models;
using System.ComponentModel.DataAnnotations;
namespace SehhaTech.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReceptionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReceptionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            return Ok(new
            {
                message = "Reception dashboard works"
            });
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var tenantId = (int)HttpContext.Items["TenantId"]!; 

            var patients = await _context.Patients
                .Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.FullName,
                    p.Phone,
                    p.Email,
                    p.Gender,
                    p.DateOfBirth,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Patients retrieved successfully",
                count = patients.Count,
                data = patients
            });
        }
        [HttpGet("patients/{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid patient id"
                });
            }

            var tenantId = (int)HttpContext.Items["TenantId"]!; 

            var patient = await _context.Patients
                .Where(p => p.Id == id && p.TenantId == tenantId)
                .Select(p => new
                {
                    p.Id,
                    p.FullName,
                    p.Phone,
                    p.Email,
                    p.Gender,
                    p.DateOfBirth,
                    p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Patient retrieved successfully",
                data = patient
            });
        }
        [HttpPost("patients")]
        public async Task<IActionResult> AddPatient(CreatePatientRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            if (request.DateOfBirth.Date >= DateTime.Today)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Date of birth must be in the past"
                });
            }

            if (request.DateOfBirth.Date < DateTime.Today.AddYears(-120))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid date of birth"
                });
            }

            var tenantId = (int)HttpContext.Items["TenantId"]!;

            var phone = request.Phone.Trim();
            var email = request.Email.Trim().ToLower();
            var gender = request.Gender.Trim();

            var phoneExists = await _context.Patients
                .AnyAsync(p => p.TenantId == tenantId && p.Phone == phone);

            if (phoneExists)
            {
                return Conflict(new
                {
                    success = false,
                    message = "A patient with this phone number already exists"
                });
            }

            var emailExists = await _context.Patients
                .AnyAsync(p => p.TenantId == tenantId && p.Email == email);

            if (emailExists)
            {
                return Conflict(new
                {
                    success = false,
                    message = "A patient with this email already exists"
                });
            }

            var patient = new Patient
            {
                FullName = request.FullName.Trim(),
                Phone = phone,
                Email = email,
                DateOfBirth = request.DateOfBirth.Date,
                Gender = gender,
                TenantId = tenantId
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, new
            {
                success = true,
                message = "Patient added successfully",
                data = new
                {
                    patient.Id,
                    patient.FullName,
                    patient.Phone,
                    patient.Email,
                    patient.Gender,
                    patient.DateOfBirth,
                    patient.CreatedAt
                }
            });
        }
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments(DateTime? from,DateTime? to,
              int page = 1,
              int pageSize = 10)
        {
            if (page <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page must be greater than 0"
                });
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "PageSize must be between 1 and 100"
                });
            }

            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "From date cannot be after To date"
                });
            }

            var tenantId = (int)HttpContext.Items["TenantId"]!;

            var query = _context.Appointments
                .Where(a => a.TenantId == tenantId);

            if (from.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(a => a.AppointmentDate < to.Value.Date.AddDays(1));
            }

            var totalCount = await query.CountAsync();

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.PatientId,
                    PatientName = a.Patient != null ? a.Patient.FullName : null,
                    a.DoctorId,
                    DoctorSpecialization = a.Doctor != null ? a.Doctor.Specialization : null,
                    a.AppointmentDate,
                    a.Duration,
                    Status = a.Status.ToString(),
                    a.Notes,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Appointments retrieved successfully",
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                count = appointments.Count,
                data = appointments
            });
        }
        [HttpPost("appointments")]
        public async Task<IActionResult> BookAppointment(CreateAppointmentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request body is required"
                });
            }

            if (request.PatientId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Valid PatientId is required"
                });
            }

            if (request.DoctorId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Valid DoctorId is required"
                });
            }

            if (request.AppointmentDate == default)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Appointment date is required"
                });
            }

            if (request.AppointmentDate <= DateTime.Now)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Appointment date must be in the future"
                });
            }

            if (request.Duration <= TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Appointment duration is required"
                });
            }

            if (request.Duration.TotalMinutes < 10 || request.Duration.TotalMinutes > 180)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Appointment duration must be between 10 and 180 minutes"
                });
            }

            var tenantId = (int)HttpContext.Items["TenantId"]!; 

            var patientExists = await _context.Patients
                .AnyAsync(p => p.Id == request.PatientId && p.TenantId == tenantId);

            if (!patientExists)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Patient not found"
                });
            }

            var doctorExists = await _context.Doctors
                .AnyAsync(d => d.Id == request.DoctorId && d.TenantId == tenantId && d.IsActive);

            if (!doctorExists)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Doctor not found or inactive"
                });
            }

            var appointmentStart = request.AppointmentDate;
            var appointmentEnd = request.AppointmentDate.Add(request.Duration);

            var hasConflict = await _context.Appointments
                .AnyAsync(a =>
                    a.TenantId == tenantId &&
                    a.DoctorId == request.DoctorId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    appointmentStart < a.AppointmentDate.Add(a.Duration) &&
                    appointmentEnd > a.AppointmentDate);

            if (hasConflict)
            {
                return Conflict(new
                {
                    success = false,
                    message = "Doctor already has an appointment during this time"
                });
            }

            var appointment = new Appointment
            {
                TenantId = tenantId,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                Duration = request.Duration,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                Status = AppointmentStatus.Scheduled
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                success = true,
                message = "Appointment booked successfully",
                data = new
                {
                    appointment.Id,
                    appointment.PatientId,
                    appointment.DoctorId,
                    appointment.AppointmentDate,
                    appointment.Duration,
                    status = appointment.Status.ToString(),
                    appointment.Notes,
                    appointment.CreatedAt
                }
            });
        }
        [HttpPut("appointments/{id}/checkin")]
        public async Task<IActionResult> CheckInAppointment(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid appointment id"
                });
            }

            var tenantId = (int)HttpContext.Items["TenantId"]!; 

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

            if (appointment == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Appointment not found"
                });
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot check in a cancelled appointment"
                });
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot check in a completed appointment"
                });
            }

            if (appointment.Status == AppointmentStatus.Confirmed)
            {
                return Conflict(new
                {
                    success = false,
                    message = "Patient is already checked in"
                });
            }

            appointment.Status = AppointmentStatus.Confirmed;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Patient checked in successfully",
                data = new
                {
                    appointment.Id,
                    status = appointment.Status.ToString()
                }
            });
        }
        [HttpGet("queue")]
        public async Task<IActionResult> GetTodayQueue()
        {
            var tenantId = (int)HttpContext.Items["TenantId"]!;

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var queue = await _context.Appointments
                .Where(a =>
                    a.TenantId == tenantId &&
                    a.AppointmentDate >= today &&
                    a.AppointmentDate < tomorrow &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.Completed &&
                    a.Status != AppointmentStatus.NoShow)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new
                {
                    appointmentId = a.Id,

                    patient = new
                    {
                        id = a.PatientId,
                        fullName = a.Patient != null ? a.Patient.FullName : null,
                        phone = a.Patient != null ? a.Patient.Phone : null,
                        email = a.Patient != null ? a.Patient.Email : null
                    },

                    doctor = new
                    {
                        id = a.DoctorId,
                        specialization = a.Doctor != null ? a.Doctor.Specialization : null,
                        isActive = a.Doctor != null ? a.Doctor.IsActive : false
                    },

                    appointmentDate = a.AppointmentDate,
                    appointmentTime = a.AppointmentDate.ToString("HH:mm"),
                    duration = a.Duration,
                    status = a.Status.ToString(),
                    notes = a.Notes,
                    waitingMinutes = a.Status == AppointmentStatus.Confirmed
                        ? (int)Math.Max(0, (DateTime.Now - a.AppointmentDate).TotalMinutes)
                        : 0
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = queue.Any()
                    ? "Today queue retrieved successfully"
                    : "No patients in today's queue",
                date = today.ToString("yyyy-MM-dd"),
                count = queue.Count,
                data = queue
            });
        }

        [HttpGet("doctors/available")]
        public async Task<IActionResult> GetAvailableDoctors()
        {
            var tenantId = (int)HttpContext.Items["TenantId"]!; // Temporary for Swagger testing only

            var doctors = await _context.Doctors
                .Where(d => d.TenantId == tenantId && d.IsActive)
                .OrderBy(d => d.Specialization)
                .Select(d => new
                {
                    id = d.Id,
                    specialization = d.Specialization,
                    bio = d.Bio,
                    profileImageUrl = d.ProfileImageUrl,
                    isActive = d.IsActive,
                    user = d.User == null ? null : new
                    {
                        id = d.User.Id,
                        fullName = d.User.FullName,
                        email = d.User.Email,
                        profileImageUrl = d.User.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = doctors.Any()
                    ? "Available doctors retrieved successfully"
                    : "No available doctors found",
                count = doctors.Count,
                data = doctors
            });
        }


        //[HttpPost("test-doctor")]
        //public async Task<IActionResult> AddTestDoctor()
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync();

        //    if (user == null)
        //    {
        //        return BadRequest(new
        //        {
        //            message = "No user found. Please register a user first."
        //        });
        //    }

        //    var doctor = new Doctor
        //    {
        //        TenantId = user.TenantId ?? 1,
        //        UserId = user.Id,
        //        Specialization = "General Medicine",
        //        Bio = "Test doctor",
        //        IsActive = true
        //    };

        //    _context.Doctors.Add(doctor);
        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        message = "Test doctor added successfully",
        //        doctorId = doctor.Id,
        //        userId = user.Id,
        //        tenantId = doctor.TenantId
        //    });
        //}
   
    }


    public class CreatePatientRequest
    {
        [Required(ErrorMessage = "Patient full name is required")]
        [MinLength(3, ErrorMessage = "Patient full name must be at least 3 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Patient phone is required")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Patient phone must contain digits only and be between 10 and 15 digits")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Patient email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Patient date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Patient gender is required")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be Male or Female")]
        public string Gender { get; set; } = string.Empty;
    }
    public class CreateAppointmentRequest
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Notes { get; set; }
    }
}