# WingDingFork
A BrainFuck interpreter but it's WingDings

### Running:
```
.\app\dingfork.exe
```

### Usage:
Enter or paste your [code] from the following instructions:
```
+ : 👆︎ Increment the byte at the data pointer by one.
- : 👇︎ Decrement the byte at the data pointer by one.
. : 🖳 Output the byte at the data pointer.
, : ✂ Accept one byte of input, storing its value in the byte at the data pointer.
< : 👈︎ Decrement the data pointer by one (to point to the next cell to the left).
> : 👉︎ Increment the data pointer by one (to point to the next cell to the right).
] : 🗀 If the byte at the data pointer is nonzero, then instead of moving the instruction pointer
       forward to the next command, jump it back to the command after the matching [ command.[a]
[ : 🗁 If the byte at the data pointer is zero, then instead of moving the instruction pointer
       forward to the next command, jump it forward to the command after the matching ] command.
```

### Example Program:

![Hello World](./ref/example.png)

### Next:
        * Clean up TODOs
        
        * Update to use the WingDings as the actual instructions in code
            - Create mapping file instead of hard-coding instruction dict

                [keymap.csv]
                    "]", "🗀  "
                    "[", "🗁  "
                    "<", "👈︎ "
                    ">", "👉︎ "
                    ".", "🖳  "
                    ",", "✂  "
                    "+", "👆︎ "
                    "-", "👇︎ "
            
        * Add sub-routines as text files with WingDing names
            - Add ability to save current code as a new sub-routine

                WingDingFork/subroutines/
                    🕿.txt
                    🖏.txt
                    
                [🕿.txt]
                    >++++++++[<+++++++++>-]<.
                    >++++[<+++++++>-]<+.
                    +++++++..
                    +++.
                    >>++++++[<+++++++>-]<++.
                    ------------.>++++++[<+++++++++>-]<+.
                    <.+++.------.
                    --------.
                    >>>++++[<++++++++>-]<+.
            

        * Text to WingDing code converter?
