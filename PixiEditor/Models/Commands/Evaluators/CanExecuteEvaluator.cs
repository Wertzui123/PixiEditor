﻿namespace PixiEditor.Models.Commands.Evaluators
{
    public class CanExecuteEvaluator : Evaluator<bool>
    {
        public static CanExecuteEvaluator AlwaysTrue { get; } = new StaticValueEvaluator(true);

        public static CanExecuteEvaluator AlwaysFalse { get; } = new StaticValueEvaluator(false);

        private class StaticValueEvaluator : CanExecuteEvaluator
        {
            private readonly bool value;

            public StaticValueEvaluator(bool value)
            {
                this.value = value;
            }

            public override bool CallEvaluate(Command command, object parameter) => value;
        }
    }
}
