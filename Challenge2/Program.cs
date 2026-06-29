// ============================================================
// Module 1 – C# 14 Essentials: Challenge 2
// Student : Abebe Mihiretu Demessa
// ID      : qiyas-2026-004123
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

// ----------------------------------------------------------
// Verification Data
// Two separate lists — students and enrollment records.
// The LINQ query must JOIN them together before filtering.
// ----------------------------------------------------------
var records = new List<EnrollmentRecord>
{
    new("S1", "CS1", DateTime.UtcNow),
    new("S1", "CS2", DateTime.UtcNow),
    new("S2", "CS1", DateTime.UtcNow),
    new("S3", "CS1", DateTime.UtcNow),
    new("S3", "CS2", DateTime.UtcNow),
    new("S4", "CS1", DateTime.UtcNow),
    new("S4", "CS2", DateTime.UtcNow)
};

var testStudents = new List<Student>
{
    new Student("S1", "Abeba")  { Age = 22, GPA = 3.9m }, // Should qualify — High Honors
    new Student("S2", "Kidane") { Age = 21, GPA = 2.4m }, // Excluded — GPA too low
    new Student("S3", "Dawit")  { Age = 19, GPA = 3.7m }, // Excluded — under 20
    new Student("S4", "Sara")   { Age = 23, GPA = 3.6m }  // Should qualify — Honors
};

// ----------------------------------------------------------
// LINQ Query — Grant Eligibility Report
//
// What this query does step by step:
// 1. For each student, count how many enrollment records
//    belong to them (joining two lists using Count + lambda)
// 2. Filter out students under 20 or GPA below 3.0
// 3. Filter out students with fewer than 2 enrollments
// 4. Project into a clean anonymous type: Name, GPA, Count
// 5. Group by academic band:
//      GPA >= 3.8 → "High Honors"
//      GPA >= 3.5 → "Honors"
//      GPA >= 3.0 → "Dean's List"
// 6. Sort groups alphabetically by band name
// 7. Sort students within each group by GPA descending
// ----------------------------------------------------------
var report = testStudents
    // STEP 1: Project each student with their enrollment count
    // We count once here so we don't scan the records list twice
    .Select(s => new
    {
        Student = s,
        EnrollmentCount = records.Count(r => r.StudentId == s.Id)
    })
    // STEP 2: Filter — age >= 20, GPA >= 3.0, enrollments >= 2
    .Where(x => x.Student.Age >= 20
             && x.Student.GPA >= 3.0m
             && x.EnrollmentCount >= 2)
    // STEP 3: Project into the final shape we want to display
    .Select(x => new
    {
        x.Student.Name,
        x.Student.GPA,
        EnrollmentCount = x.EnrollmentCount
    })
    // STEP 4: Group by academic band using a conditional expression
    .GroupBy(s => s.GPA >= 3.8m ? "High Honors" :
                  s.GPA >= 3.5m ? "Honors" : "Dean's List")
    // STEP 5: Sort groups alphabetically, students inside by GPA descending
    .OrderBy(g => g.Key)
    .Select(g => new
    {
        Band     = g.Key,
        Students = g.OrderByDescending(s => s.GPA).ToList()
    })
    .ToList();

// ----------------------------------------------------------
// Print the Report
// ----------------------------------------------------------
foreach (var group in report)
{
    Console.WriteLine($"\n--- {group.Band} ---");
    foreach (var s in group.Students)
    {
        Console.WriteLine($"{s.Name} | GPA: {s.GPA} | Courses: {s.EnrollmentCount}");
    }
}

Console.WriteLine("\nChallenge 2: COMPLETE");

// ----------------------------------------------------------
// EnrollmentRecord
// A simple record that represents one enrollment entry.
// It stores which student enrolled in which course and when.
// ----------------------------------------------------------
public record EnrollmentRecord(string StudentId, string CourseCode, DateTime EnrolledAt);

// ----------------------------------------------------------
// Student Class (reused from Challenge 1)
// We need this here because the LINQ query works against
// a list of Student objects.
// ----------------------------------------------------------
public class Student
{
    public string  Id   { get; }
    public string  Name { get; }
    public decimal GPA  { get; set; }
    public int     Age  { get; set; }

    public Student(string id, string name)
    {
        // Reject empty or whitespace names immediately
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Name cannot be null or whitespace.", nameof(name));

        Id   = id;
        Name = name;
    }
}