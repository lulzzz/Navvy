﻿using System;

namespace Manisero.Navvy
{
    public class TaskExecutionException : Exception
    {
        public string StepName { get; }

        public object AdditionalData { get; }

        public TaskExecutionException(
            Exception innerException,
            ITaskStep taskStep,
            object additionalData = null)
            : base("Error while executing task. See inner exception.", innerException)
        {
            StepName = taskStep.Name;
            AdditionalData = additionalData;
        }
    }
}
