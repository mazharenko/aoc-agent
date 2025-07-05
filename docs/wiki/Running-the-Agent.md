The Agent is designed to assist with the Advent of Code solving process. It will hardly perform any work if all the available puzzles are already solved and no new solutions are implemented.

It currently has no command-line arguments for running a specific day or part, or benchmarking all the solutions.

So, it's supposed to be used the following way:

1. Save the `session` cookie value from https://adventofcode.com/ to the `SESSION.COOKIE` file. If the file does not exist, the agent will prompt for its value when run for the first time.
 
    <video src="https://github.com/mazharenko/aoc-agent/assets/5635071/e644c7b3-d458-4587-b4e5-f8bd8ff8c054"></video>

2. Run the program with `dotnet watch`. You can run it as it is, but in this case the agent will process your solutions once and exit. With `dotnet watch` it will restart as soon as you fix your implementation, or add another example or day.

3. Enjoy the ASCII art of the star you've just acquired and a new toy on the Christmas tree

    <video src="https://github.com/user-attachments/assets/fb031f1c-740d-46a6-b463-69f691a4acc0"></video>