Normally, agent attempts to calculate and submit answers only when all the provided examples have passed. Until examples are given, no attempts are made.

If you feel confident enough, you can decorate a part or a day with the `[BypassNoExamples]` attribute. In this case the agent will still verify the provided examples but will not complain if none are given.