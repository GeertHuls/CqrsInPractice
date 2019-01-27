using Api.Dtos;
using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Logic.AppServices
{
    public interface ICommand
    {
    }

    public interface IQuery<TResult>
    {
    }

    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Result Handle(TCommand command);
    }

    public interface IQueryHandler<in TQuery, out TResult> where TQuery : IQuery<TResult>
    {
        TResult Handle(TQuery query);
    }

    public sealed class GetListQuery : IQuery<List<StudentDto>>
    {
        public string EnrolledIn { get; }
        public int? NumberOfCources { get; }

        public GetListQuery(string enrolledIn, int? numberOfCources)
        {
            EnrolledIn = enrolledIn;
            NumberOfCources = numberOfCources;
        }
    }

    public sealed class GetListQueryHandler : IQueryHandler<GetListQuery, List<StudentDto>>
    {
        private readonly UnitOfWork _unitOfWork;

        public GetListQueryHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<StudentDto> Handle(GetListQuery query)
        {
            return new StudentRepository(_unitOfWork)
                .GetList(query.EnrolledIn, query.NumberOfCources)
                .Select(x => ConvertToDto(x))
                .ToList();
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
    }

    public sealed class EditPersonalInfoCommand : ICommand
    {
        public long Id { get; }
        public string Name { get; }
        public string Email { get; }

        public EditPersonalInfoCommand(long id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }
    }

    public sealed class EditPersonalInfoCommandHandler : ICommandHandler<EditPersonalInfoCommand>
    {
        private readonly UnitOfWork _unitOfWork;

        public EditPersonalInfoCommandHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Result Handle(EditPersonalInfoCommand command)
        {
            var studentRepository = new StudentRepository(_unitOfWork);

            Student student = studentRepository.GetById(command.Id);
            if (student == null)
                return Result.Fail($"No student found for Id {command.Id}");

            student.Name = command.Name;
            student.Email = command.Email;

            _unitOfWork.Commit();

            return Result.Ok();
        }
    }
}
