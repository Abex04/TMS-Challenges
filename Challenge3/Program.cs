// ============================================================
// Module 1 – C# 14 Essentials: Challenge 3
// Student : Abebe Mihiretu Demessa
// ID      : qiyas-2026-004123
// ============================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ----------------------------------------------------------
// Verification / Top-Level Code
// This runs first — creates the service, wires up the event,
// and calls EnrollBatchAsync to process all students.
// ----------------------------------------------------------

var service = new EnrollmentService();

// Wire up the audit listener
// This is completely independent of the enrollment logic.
// The service does not know or care who is listening.
service.RegistrationCompleted += (sender, record) =>
    Console.WriteLine(
        $"[AUDIT] {record.StudentId} enrolled in {record.CourseCode} " +
        $"at {record.EnrolledAt:HH:mm:ss}");

var students = new List<Student>
{
    new Student("S1", "Abeba"),
    new Student("S2", "Kidane"),
    new Student("S3", "Sara")
};

// Only 2 seats — one student must fail
var course = new Course { Code = "CS-101", Title = "C# Basics", Capacity = 2 };

var result = await service.EnrollBatchAsync(students, course);

Console.WriteLine($"\nSuccessful enrollments : {result.Successes.Count}");
Console.WriteLine($"Failed enrollments     : {result.Errors.Count}");

foreach (var error in result.Errors)
    Console.WriteLine($"[ERROR] {error}");

// Assertions
if (result.Successes.Count != 2)
    Console.WriteLine("FAIL: Expected 2 successful enrollments.");
else
    Console.WriteLine("PASS: Correct number of successes.");

if (result.Errors.Count != 1)
    Console.WriteLine("FAIL: Expected 1 error.");
else
    Console.WriteLine("PASS: Correct number of errors.");

Console.WriteLine("\nChallenge 3: verification complete.");

// ----------------------------------------------------------
// EnrollmentRecord
// Represents a single successful enrollment.
// ----------------------------------------------------------
public record EnrollmentRecord(string StudentId, string CourseCode, DateTime EnrolledAt);

// ----------------------------------------------------------
// Student Class
// Represents a student in the system.
// ----------------------------------------------------------
public class Student
{
    public string Id   { get; }
    public string Name { get; }

    public Student(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Name cannot be null or whitespace.", nameof(name));
        Id   = id;
        Name = name;
    }
}

// ----------------------------------------------------------
// Course Class
// Represents a course with a limited number of seats.
// ----------------------------------------------------------
public class Course
{
    public required string Code     { get; init; }
    public required string Title    { get; init; }
    public int             Capacity { get; set; }
}

// ----------------------------------------------------------
// TmsException — Base Exception
// All TMS domain exceptions extend this class.
// Having a base class lets callers catch all TMS errors
// with a single catch (TmsException) if needed.
// ----------------------------------------------------------
public class TmsException : Exception
{
    // Constructor 1: message only
    public TmsException(string message) : base(message) { }

    // Constructor 2: message + inner exception (for wrapping)
    public TmsException(string message, Exception inner) : base(message, inner) { }
}

// ----------------------------------------------------------
// CourseRegistrationException
// Thrown when a student cannot be enrolled because the
// course is full. Carries StudentId and CourseCode so the
// error message is meaningful and traceable.
// ----------------------------------------------------------
public class CourseRegistrationException : TmsException
{
    public string StudentId  { get; }
    public string CourseCode { get; }

    public CourseRegistrationException(string studentId, string courseCode)
        : base($"Cannot enroll student {studentId} in {courseCode}: course is full.")
    {
        StudentId  = studentId;
        CourseCode = courseCode;
    }
}

// ----------------------------------------------------------
// BatchResult
// Returned by EnrollBatchAsync to report what happened.
// Successes = list of successful enrollment records
// Errors    = list of error messages for failed enrollments
// ----------------------------------------------------------
public record BatchResult(
    IReadOnlyList<EnrollmentRecord> Successes,
    IReadOnlyList<string>           Errors);

// ----------------------------------------------------------
// EnrollmentService
// Processes a batch of enrollments concurrently.
//
// KEY RULES:
// 1. One failure must never stop the others — each task
//    handles its own exception internally.
// 2. Every success raises the RegistrationCompleted event
//    so the audit stream can record it independently.
// 3. We use ConcurrentBag because multiple tasks write to
//    the collections at the same time — List<T> is not
//    safe for concurrent writes.
// ----------------------------------------------------------
public class EnrollmentService
{
    // Event that fires after every successful enrollment.
    // Anyone can listen — the service does not know who.
    public event EventHandler<EnrollmentRecord>? RegistrationCompleted;

    public async Task<BatchResult> EnrollBatchAsync(
        IEnumerable<Student> students,
        Course course)
    {
        // Thread-safe collections for concurrent writes
        var successes = new ConcurrentBag<EnrollmentRecord>();
        var errors    = new ConcurrentBag<string>();

        // Project each student into a Task that runs concurrently
        var tasks = students.Select(async student =>
        {
            try
            {
                // Simulate I/O delay (e.g. database write)
                await Task.Delay(50);

                // Check if there is a seat available
                if (course.Capacity <= 0)
                    throw new CourseRegistrationException(student.Id, course.Code);

                // Claim a seat and create the enrollment record
                course.Capacity--;
                var record = new EnrollmentRecord(student.Id, course.Code, DateTime.UtcNow);

                // Add to successes
                successes.Add(record);

                // Raise the event — notify all listeners (e.g. audit stream)
                // The ?. handles the case where no one is listening
                RegistrationCompleted?.Invoke(this, record);
            }
            catch (CourseRegistrationException ex)
            {
                // Handle failure internally — do NOT re-throw
                // This ensures one failure never aborts the others
                errors.Add(ex.Message);
            }
        });

        // Run all tasks concurrently and wait for all to finish
        await Task.WhenAll(tasks);

        // Return the final result with both collections
        return new BatchResult(
            successes.ToList(),
            errors.ToList());
    }
}