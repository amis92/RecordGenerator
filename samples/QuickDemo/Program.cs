using System;
using Amadevus.RecordGenerator;

namespace QuickDemo
{
    [Record]
    public sealed partial class Contact
    {
        public int Id { get; }
        public string Name { get; }
        public string Email { get; }
        public DateTime? Birthday { get; }
    }

    public static class Program
    {
        public static void Main()
        {
            var adam = new Contact.Builder
            {
                Id = 1,
                Name = "Adam Demo",
                Email = "foo@bar.com"
            }.ToImmutable();
            var adamWithBday = adam.WithBirthday(DateTime.UtcNow);
            Console.WriteLine("Pretty display: " + adamWithBday);
            // Pretty display: { Id = 1, Name = Adam Demo, Email = foo@bar.com, Birthday = 06.01.2020 23:17:06 }
            Console.WriteLine("Check equality: " + adam.Equals(adamWithBday));
            // Check equality: False
            Console.WriteLine("Check equality: " + adam.Equals(new Contact(1, "Adam Demo", "foo@bar.com", null)));
            // Check equality: True
        }
    }
}