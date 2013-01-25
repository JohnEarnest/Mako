MakoForth Reference Guide
=========================
MakoForth is a small Forth dialect which should be moderately familiar to anyone who has been exposed to ANS-Forth. The major notable differences are the use of `#` as line comments, `loop...(break)...again|until|while` looping constructs and the inclusion of syntax for inline double-quoted string constants. Cells are always 32-bit two's complement signed integers.

Conventions
-----------
- `a`,`b` and `n` are signed 32-bit numbers (or any value)
- `str` is the address of a null (0) terminated string
- `f` is a bit-flag- 0 for false and -1 for true
- `addr` generally refers to an address

Words with starred stack effects are *immediate* and will execute when encountered rather than being compiled.

Core Vocabulary
---------------
- `+`         `( a b -- n )`     add `a` to `b`
- `-`         `( a b -- n )`     subtract `b` from `a`
- `*`         `( a b -- n )`     multiply `a` by `b`
- `/`         `( a b -- n )`     divide `a` by `b`
- `mod`       `( a b -- n )`     compute the modulus of `a` by `b`
- `and`       `( a b -- n )`     bitwise and
- `or`        `( a b -- n )`     bitwise or
- `xor`       `( a b -- n )`     bitwise xor
- `not`       `( a -- n )`       bitwise not
- `<`         `( a b -- f )`     is `a` less than `b`?
- `>`         `( a b -- f )`     is `a` greater than `b`?
- `!`         `( n addr -- )`    write value `n` to an address in memory
- `@`         `( addr -- n )`    read a value from an address in memory
- `dup`       `( a -- a a )`     duplicate the top stack element
- `over`      `( a b -- a b a )` duplicate the second stack element
- `drop`      `( a -- )`         discard the top stack element
- `swap`      `( a b -- b a )`   exchange the top stack elements
- `>r`        `( n -- )`         move the top stack element to the rstack
- `r>`        `( -- n )`         move the top rstack element to the stack
- `i`         `( -- n )`         copy the top rstack element to the stack
- `j`         `( -- n )`         copy the second rstack element to the stack
- `here`      `( -- addr )`      obtain the address of a var containing the end of the dictionary
- `mode`      `( -- f )`         obtain true if compiling, false if interpreting
- `,`         `( n -- )`         append a value to the dictionary and advance `here`
- `]`         `( -- )`           switch to compiling mode
- `[`         `( -- )*`          switch to interpreting mode
- `immediate` `( -- )`           make the last defined word immediate
- `literal`   `( n -- )*`        compile a constant on the stack into the dictionary definition
- `create`    `( -- )`           read a name from the input stream and make a new dictionary entry
- `exit`      `( -- )*`          compile a return but stay in compiling mode
- `:`         `( -- )`           essentially `create ]`- define a procedure
- `;`         `( -- )*`          terminate the current dictionary definition
- `if`        `( -- )*`          compile an `if` (takes an argument from the stack)
- `then`      `( -- )*`          complete an `if` statement.
- `else`      `( -- )*`          place between `if` and `then` optionally
- `loop`      `( -- )*`          create the head of a loop
- `again`     `( -- )*`          complete a `loop` and branch back unconditionally
- `while`     `( -- )*`          complete a `loop` and branch back if a stack value is true
- `until`     `( -- )*`          complete a `loop` and branch back if a stack value is false
- `break`     `( -- )*`          unconditionally exit the current `loop`
- `'`         `( -- )*`          compile a literal giving the address of a named word
- `does>`     `( -- )`           assign the remainder of this definition as behavior for the current word

Text Output
-----------
- `.`         `( n -- )`         print a signed number followed by a space
- `space`     `( -- )`           print a space
- `cr`        `( -- )`           print a carriage return
- `type`      `( str -- )`       print a string
- `typeln`    `( str -- )`       print a string and a carriage return

Programming Helpers
-------------------
- `forget`    `( -- )`           read a name and forget it and all following words
- `free`      `( -- )`           print remaining free dictionary space
- `see`       `( -- )`           read a name and print a dissassembly of that word
- `#`         `( -- )*`          ignore input until a newline- line comment
- `(`         `( -- )*`          ignore input until a `)`- block comment
- `stack`     `( -- )`           print the contents of the stack
- `words`     `( -- )`           print a list of all dictionary entries







