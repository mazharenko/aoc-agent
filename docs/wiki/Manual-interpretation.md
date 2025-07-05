Using the Agent library implies calculating an answer, formatting and submitting it automatically. However, sometimes the formatting method can be significantly more [difficult](https://www.reddit.com/r/adventofcode/comments/zhl2wl/2022_day_10_part_2_entering_the_answer_for_part_2/) to implement than the actual puzzle solution.

```plain
####.###..#..#.###..#..#.####..##..#..#.
#....#..#.#..#.#..#.#..#....#.#..#.#..#.
###..###..#..#.#..#.####...#..#....####.
#....#..#.#..#.###..#..#..#...#....#..#.
#....#..#.#..#.#.#..#..#.#....#..#.#..#.
#....###...##..#..#.#..#.####..##..#..#.
```

Implementing OCR functionality can be fun as it seems that the used pattern remains the same through years. Still, the library offers the way of manual interpretation of the result.

Decorate a day or a part with the `[ManualInterpretation]` attribute. As a result, the agent will just print out the calculated result and prompt the user to provide the string to try to submit.
