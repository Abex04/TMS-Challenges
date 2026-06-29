

## Overview
This repository contains three advanced C# 14 challenges built around a
Training Management System (TMS) domain. Each challenge focuses on a
different set of modern C# features and programming principles.

---

## Challenge 1 — Immutability, Safety & Custom Enums
**Folder:** `Challenge1/`

### What it does
- Defines a `CourseStatus` enum with three states: `Active`, `Suspended`, and `Archived`
- Uses the **C# 14 `field` keyword** inside the `Status` setter to automatically
  zero out `Capacity` when a course is archived
- Implements a `GradeRecord` as a `readonly record struct` with score validation
  at construction time
- Implements a `Student` class that rejects null or empty names immediately

### Key C# Features Used
- C# 14 `field` keyword
- `readonly record struct`
- Fail-fast constructor validation
- `IReadOnlyList<T>` for encapsulation

### Result
PASS: Archived course has zero capacity.
PASS: Empty name correctly rejected.
PASS: Valid student created.
PASS: Invalid score correctly rejected.
PASS: Value equality confirmed.

## Challenge 2 — Multi-Criteria LINQ Reporter
**Folder:** `Challenge2/`

### What it does
- Joins students and enrollment records from two separate collections
- Filters students by age, GPA, and enrollment count
- Groups qualifying students into academic bands:
  - GPA >= 3.8 → High Honors
  - GPA >= 3.5 → Honors
  - GPA >= 3.0 → Dean's List
- Sorts groups alphabetically and students within groups by GPA descending

### Key C# Features Used
- LINQ `Select`, `Where`, `GroupBy`, `OrderBy`, `OrderByDescending`
- Anonymous types
- Lambda expressions
- Method chaining

### Result
--- High Honors ---
Abeba | GPA: 3.9 | Courses: 2
--- Honors ---
Sara | GPA: 3.6 | Courses: 2
## Challenge 3 — Async Batch Operations & Event Decoupling
**Folder:** `Challenge3/`

### What it does
- Processes a batch of student enrollments concurrently using `Task.WhenAll`
- Each task handles its own failure internally so one failure never stops the others
- Raises a `RegistrationCompleted` event after every successful enrollment
  so the audit stream can record it independently
- Uses `ConcurrentBag<T>` for thread-safe writes across concurrent tasks

### Key C# Features Used
- `async` / `await`
- `Task.WhenAll`
- `EventHandler<T>` events
- `ConcurrentBag<T>`
- Custom exception hierarchy (`TmsException` → `CourseRegistrationException`)

### Result

[AUDIT] S3 enrolled in CS-101 at 21:39:13
[AUDIT] S1 enrolled in CS-101 at 21:39:13
Successful enrollments : 2
Failed enrollments     : 1
[ERROR] Cannot enroll student S2 in CS-101: course is full.
PASS: Correct number of successes.
PASS: Correct number of errors.
