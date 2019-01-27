using Api.Dtos;
using CSharpFunctionalExtensions;
using Logic.AppServices;
using Logic.Students;
using Logic.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Driver;

namespace Api.Controllers
{
    [Route("api/students")]
    public sealed class StudentController : BaseController
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly Messages _messages;
        private readonly StudentRepository _studentRepository;
        private readonly CourseRepository _courseRepository;

        public StudentController(UnitOfWork unitOfWork, Messages messages)
        {
            _unitOfWork = unitOfWork;
            _studentRepository = new StudentRepository(unitOfWork);
            _courseRepository = new CourseRepository(unitOfWork);
            _messages = messages;
        }

        [HttpGet]
        public IActionResult GetList(string enrolled, int? number)
        {
            var list = _messages.Dispatch(new GetListQuery(enrolled, number));
            return Ok(list);
        }

        private StudentDto ConvertToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Course1 = student.FirstEnrollment?.Course?.Name,
                Course1Grade = student.FirstEnrollment?.Grade.ToString(),
                Course1Credits = student.FirstEnrollment?.Course?.Credits,
                Course2 = student.SecondEnrollment?.Course?.Name,
                Course2Grade = student.SecondEnrollment?.Grade.ToString(),
                Course2Credits = student.SecondEnrollment?.Course?.Credits,
            };
        }

        [HttpPost]
        public IActionResult Register([FromBody] NewStudentDto dto)
        {
            var command = new RegisterCommand(dto.Name, dto.Email,
                dto.Course1, dto.Course1Grade,
                dto.Course2, dto.Course2Grade);

            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        [HttpDelete("{id}")]
        public IActionResult Unregister(long id)
        {
            var command = new UnregisterCommand(id);

            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        [HttpPost("{id}/enrollments")]
        public IActionResult Enroll(long id, [FromBody] StudentEnrollmentDto dto)
        {
            var command = new EnrollCommand(id, dto.Course, dto.Grade);

            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        [HttpPost("{id}/enrollments/{enrollmentNumber}")]
        public IActionResult Transfer(long id, int enrollmentNumber,
            [FromBody] StudentTransferDto dto)
        {
            var command = new TransferCommand(id, enrollmentNumber, dto.Course, dto.Grade);

            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        [HttpPost("{id}/enrollments/{enrollmentNumber}/deletion")]
        public IActionResult Disenroll(long id, int enrollmentNumber,
            [FromBody] StudentDisenrollmentDto dto)
        {
            var command = new DisenrollCommand(id, enrollmentNumber, dto.Comment);

            Result result = _messages.Dispatch(command);
            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        [HttpPut("{id}")]
        public IActionResult EditPersonalInfo(long id, [FromBody] StudentPersonalInfoDto dto)
        {
            var editPersonalInfoCommand = new EditPersonalInfoCommand(
                id, dto.Name, dto.Email);
            var result = _messages.Dispatch(editPersonalInfoCommand);

            return result.IsSuccess ? Ok() : Error(result.Error);
        }

        private bool HasEnrollmentChanged(string newCourseName, string newGrade, Enrollment enrollment)
        {
            if (string.IsNullOrWhiteSpace(newCourseName) && enrollment == null)
                return false;

            if (string.IsNullOrWhiteSpace(newCourseName) || enrollment == null)
                return true;

            return newCourseName != enrollment.Course.Name || newGrade != enrollment.Grade.ToString();
        }
    }
}
