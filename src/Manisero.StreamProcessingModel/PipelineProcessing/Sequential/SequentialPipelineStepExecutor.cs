﻿using System;
using System.Diagnostics;
using System.Threading;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Sequential
{
    public class SequentialPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        public void Execute(
            PipelineTaskStep<TData> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var batchNumber = 0;

            foreach (var input in step.Input)
            {
                batchNumber++;
                PipelineExecutionEvents.BatchStarted(batchNumber, input, step, context.TaskDescription, DateTimeUtils.Now);
                var batchSw = Stopwatch.StartNew();

                foreach (var block in step.Blocks)
                {
                    PipelineExecutionEvents.BlockStarted(batchNumber, input, block, step, context.TaskDescription, DateTimeUtils.Now);
                    var blockSw = Stopwatch.StartNew();

                    try
                    {
                        block.Body(input);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    blockSw.Stop();
                    PipelineExecutionEvents.BlockEnded(batchNumber, input, block, step, context.TaskDescription, blockSw.Elapsed, DateTimeUtils.Now);
                    cancellation.ThrowIfCancellationRequested();
                }

                batchSw.Stop();
                PipelineExecutionEvents.BatchEnded(batchNumber, input, step, context.TaskDescription, batchSw.Elapsed, DateTimeUtils.Now);
                StepExecutionUtils.ReportProgress(batchNumber, step.ExpectedInputBatchesCount, progress);
            }
        }
    }
}