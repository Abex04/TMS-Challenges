// ============================================================
// Module 1 – C# 14 Essentials: Challenge 1
// Student : Abebe Mihiretu Demessa
// ID      : qiyas-2026-004123
// ============================================================

using System;
using System.Collections.Generic;

// ----------------------------------------------------------
// Verification Block
// Tests all the rules we implemented above.
// Every line printed must say PASS.
// ----------------------------------------------------------
try
{
    // TEST 1: Archiving a course must zero its capacity
    var course = new Course { Code = "CS-101", Title = "C# Basics", Capacity = 10 };
    course.Status = CourseStatus.Archived;
    if (course.Capacity != 0)
        Console.WriteLine("FAIL: Capacity must drop to 0 when archived.");
    else
        Console.WriteLine("PASS: Archived course has zero capacity.");

    // TEST 2: Empty name must be rejected at construction
    try
    {
        var bad = new Student("STU-00", "");
        Console.WriteLine("FAIL: Empty name should have thrown ArgumentException.");
    }
    catch (ArgumentException)
    {
        Console.WriteLine("PASS: Empty name correctly rejected.");
    }

    // TEST 3: Valid student must construct without error
    var student = new Student("STU-99", "Abeba");
    Console.WriteLine("PASS: Valid student created.");

    // TEST 4: Score above 100 must be rejected at struct construction
    try
    {
        var badGrade = new GradeRecord("CS-101", 150m, DateTime.UtcNow);
        Console.WriteLine("FAIL: Score above 100 should have thrown.");
    }
    catch (ArgumentOutOfRangeException)
    {
        Console.WriteLine("PASS: Invalid score correctly rejected.");
    }

    // TEST 5: Two GradeRecords with same data must be equal
    var grade1 = new GradeRecord("CS-101", 95.5m, DateTime.UnixEpoch);
    var grade2 = new GradeRecord("CS-101", 95.5m, DateTime.UnixEpoch);
    if (grade1 != grade2)
        Console.WriteLine("FAIL: Struct records should be equal by value.");
    else
        Console.WriteLine("PASS: Value equality confirmed.");

    Console.WriteLine("\nChallenge 1: ALL CHECKS PASSED");
}
catch (Exception ex)
{
    Console.WriteLine($"Challenge 1: UNEXPECTED FAILURE — {ex.Message}");
}

// ----------------------------------------------------------
// CourseStatus Enum
// Defines the three possible states a course can be in.
// Active    – course is open for enrollment
// Suspended – course is temporarily unavailable
// Archived  – course is permanently closed, no seats allowed
// ----------------------------------------------------------
public enum CourseStatus
{
    Active,
    Suspended,
    Archived
}

// ----------------------------------------------------------
// Course Class
// Represents a course in the system.
// KEY RULE: When Status is set to Archived, Capacity must
// automatically drop to 0 to prevent accidental enrollments.
// We use the C# 14 `field` keyword inside the Status setter
// instead of declaring a separate backing field manually.
// ----------------------------------------------------------
public class Course
{
    public required string Code  { get; init; }  // e.g. "CS-101"
    public required string Title { get; init; }  // e.g. "C# Basics"
    public int Capacity { get; set; }            // number of available seats

    public CourseStatus Status
    {
        get => field;  // `field` is the auto-generated backing field (C# 14)
        set
        {
            field = value;                         // store the new status
            if (value == CourseStatus.Archived)
                Capacity = 0;                      // close all seats immediately
        }
    }
}

// ----------------------------------------------------------
// GradeRecord – readonly record struct
// Represents a single grade given to a student for a course.
//
// WHY readonly record struct?
// - `readonly`      : cannot be changed after creation (immutable)
// - `record`        : gives us automatic value equality, meaning
//                     two GradeRecords with the same data are equal
// - `struct`        : stored by value, not by reference
//
// KEY RULE: Score must be between 0 and 100. We validate this
// in the constructor so an invalid score is rejected BEFORE
// the struct is ever created.
// ----------------------------------------------------------
public readonly record struct GradeRecord
{
    public string   CourseCode { get; init; }
    public decimal  Score      { get; init; }
    public DateTime GradedAt   { get; init; }

    public GradeRecord(string courseCode, decimal score, DateTime gradedAt)
    {
        // Reject invalid scores before assigning anything
        if (score < 0 || score > 100)
            throw new ArgumentOutOfRangeException(
                nameof(score),
                $"Score must be between 0 and 100 inclusive. Received: {score}");

        // Only assign if validation passed
        CourseCode = courseCode;
        Score      = score;
        GradedAt   = gradedAt;
    }
}

// ----------------------------------------------------------
// Student Class
// Represents a student in the system.
//
// KEY RULES:
// 1. Name cannot be null or whitespace — we reject it
//    immediately in the constructor (fail-fast principle)
// 2. Grades are stored in a private list so nothing outside
//    this class can add or remove grades directly.
//    We expose it as IReadOnlyList so others can only READ it.
// ----------------------------------------------------------
public class Student
{
    // Private list — only this class can add grades
    private readonly List<GradeRecord> _grades = new();

    public string  Id   { get; }
    public string  Name { get; }
    public decimal GPA  { get; set; }
    public int     Age  { get; set; }

    // Exposed as read-only — callers can read but not modify
    public IReadOnlyList<GradeRecord> Grades => _grades.AsReadOnly();

    public Student(string id, string name)
    {
        // Reject empty or whitespace names immediately
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Name cannot be null or whitespace.", nameof(name));

        Id   = id;
        Name = name;
    }

    // The only way to add a grade — controlled by this class
    public void AddGrade(GradeRecord grade) => _grades.Add(grade);
}