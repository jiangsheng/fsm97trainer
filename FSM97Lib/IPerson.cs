using System;

namespace FSM97Lib
{
    public interface IPerson
    {
        string NationalityName { get; set; }
        int Age { get; set; }
        string LastName { get; set; }
        string FirstName { get; set; }
        int Nationality { get; set; }
        DateTime BirthDay { get; }
        int BirthDateOffset { get; set; }
    }
}
