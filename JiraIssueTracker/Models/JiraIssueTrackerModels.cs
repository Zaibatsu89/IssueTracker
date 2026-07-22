using System;
using System.Collections.Generic;

namespace JiraIssueTracker.Models;

public sealed class Issue
{
    public Projects Project { get; init; }

    public int Code { get; init; }

    public string Title { get; init; } = string.Empty;

    public string AreaTag { get; init; } = string.Empty;

    public string PriorityTag { get; init; } = string.Empty;

    public string AssignedTo { get; init; } = string.Empty;

    public string EnvironmentName { get; init; } = string.Empty;

    public Step Step { get; init; } = new();
}

public sealed class Step
{
    public StepTypes Type { get; init; }

    public IReadOnlyList<Part> Parts { get; init; } = Array.Empty<Part>();

    public string Effort { get; init; } = string.Empty;

    public string Elapsed { get; init; } = string.Empty;
}

public sealed class Part
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<TestAction> Actions { get; init; } = Array.Empty<TestAction>();
}

public sealed class TestAction
{
    public string Name { get; init; } = string.Empty;

    public IReadOnlyList<Check> Checks { get; init; } = Array.Empty<Check>();
}

public sealed class Check
{
    public string Name { get; set; } = string.Empty;

    public Results Result { get; set; } = Results.Unknown;

    public string Remarks { get; set; } = string.Empty;
}

public enum Projects
{
    GDAT
}

public enum StepTypes
{
    Test
}

public enum Results
{
    Unknown,
    Ok,
    Nok
}

public static class IssueFactory
{
    public static Issue CreateSample()
    {
        var waterCheck = new Check
        {
            Name = "Full water bottle installed?",
            Result = Results.Nok,
            Remarks = "Bottle sensor does not detect the inserted bottle until it is reseated."
        };

        var pumpCheck = new Check
        {
            Name = "Pump starts without unusual noise?",
            Result = Results.Ok,
            Remarks = string.Empty
        };

        var leakageCheck = new Check
        {
            Name = "No leakage visible around hose connection?",
            Result = Results.Unknown,
            Remarks = string.Empty
        };

        var preconditionsAction = new TestAction
        {
            Name = "Run preconditions",
            Checks = new[]
            {
                waterCheck,
                pumpCheck,
                leakageCheck
            }
        };

        var runTestsPart = new Part
        {
            Name = "Run tests",
            Actions = new[]
            {
                preconditionsAction
            }
        };

        var jiraStep = new Step
        {
            Type = StepTypes.Test,
            Parts = new[]
            {
                runTestsPart
            },
            Effort = "01:00",
            Elapsed = "00:05"
        };

        return new Issue
        {
            Project = Projects.GDAT,
            Code = 286,
            Title = "App - Triton testing",
            AreaTag = "HARDWARE_QA",
            PriorityTag = "BLOCKER",
            AssignedTo = "QA_LEAD_01",
            EnvironmentName = "STAGING_V2",
            Step = jiraStep
        };
    }
}
