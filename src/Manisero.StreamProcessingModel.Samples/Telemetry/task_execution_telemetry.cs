﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.BasicProcessing;
using Manisero.StreamProcessingModel.Core.Events;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Samples.Utils;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples.Telemetry
{
    public class task_execution_telemetry
    {
        [Fact]
        public void task_start_and_end_is_reported()
        {
            // Arrange
            TaskStartedEvent? startedEvent = null;
            TaskEndedEvent? endedEvent = null;

            var events = new TaskExecutionEvents();
            events.TaskStarted += x => startedEvent = x;
            events.TaskEnded += x => endedEvent = x;

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    BasicTaskStep.Empty("Step")
                }
            };
            
            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, events);

            // Act
            executor.Execute(taskDescription);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Task.ShouldBeEquivalentTo(taskDescription);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Task.ShouldBeEquivalentTo(taskDescription);
            endedEvent.Value.Duration.Ticks.Should().BePositive();
        }

        [Fact]
        public void step_start_and_end_is_reported()
        {
            // Arrange
            StepStartedEvent? startedEvent = null;
            StepEndedEvent? endedEvent = null;

            var events = new TaskExecutionEvents();
            events.StepStarted += x => startedEvent = x;
            events.StepEnded += x => endedEvent = x;

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    BasicTaskStep.Empty("Step")
                }
            };
            
            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, events);

            // Act
            executor.Execute(taskDescription);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Step.ShouldBeEquivalentTo(taskDescription.Steps[0]);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Step.ShouldBeEquivalentTo(taskDescription.Steps[0]);
            endedEvent.Value.Duration.Ticks.Should().BePositive();
        }

        [Fact]
        public void step_skip_is_reported()
        {
            // Arrange
            StepSkippedEvent? skippedEvent = null;

            var events = new TaskExecutionEvents();
            events.StepSkipped += x => skippedEvent = x;

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Step",
                        () => { },
                        _ => false)
                }
            };

            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, events);

            // Act
            executor.Execute(taskDescription);

            // Assert
            skippedEvent.Should().NotBeNull();
            skippedEvent.Value.Step.ShouldBeEquivalentTo(taskDescription.Steps[0]);
        }

        [Fact]
        public void step_cancellation_is_reported()
        {
            // Arrange
            StepCanceledEvent? canceledEvent = null;

            var events = new TaskExecutionEvents();
            events.StepCanceled += x => canceledEvent = x;

            var cancellationSource = new CancellationTokenSource();

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Step",
                        () => cancellationSource.Cancel())
                }
            };

            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, events);

            // Act
            executor.Execute(taskDescription, cancellation: cancellationSource.Token);

            // Assert
            canceledEvent.Should().NotBeNull();
            canceledEvent.Value.Step.ShouldBeEquivalentTo(taskDescription.Steps[0]);
        }

        [Fact]
        public void step_failure_is_reported()
        {
            // Arrange
            StepFailedEvent? failedEvent = null;

            var events = new TaskExecutionEvents();
            events.StepFailed += x => failedEvent = x;

            var exception = new Exception();

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Step",
                        () => throw exception)
                }
            };

            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, events);

            // Act
            executor.Execute(taskDescription);

            // Assert
            failedEvent.Should().NotBeNull();
            failedEvent.Value.Exception.InnerException.ShouldBeEquivalentTo(exception);
            failedEvent.Value.Step.ShouldBeEquivalentTo(taskDescription.Steps[0]);
        }
    }
}