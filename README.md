# NameTwister by [12noon LLC](https://12noon.com)

NameTwister can help you rename files in complicated (or easy) ways.

Drag & drop files from anywhere in Windows onto the NameTwister window. Then enter
simple search & replace text (such as replace "cat" with "dog") or more complex
[regular expressions](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions).

![NameTwister](https://raw.githubusercontent.com/skst/NameTwister/master/NameTwister.png)

In the target expression, you can use `$1` through `$9` for groups you matched in the source expression.

Right-click on either expression field for a quick reminder of the most frequently-used elements.
(This is usually easier than memorizing the arcane syntax of regular expressions.)

The dropdown combo boxes store their previous values so you can easily reuse previous expressions.

The *Case-sensitive matching* check box requires that the text will only match if the case matches.

If you tick the *Replace all matches* check box, every match in the source expression will be
replaced with the target expression. If the check box is cleared, only the first match will be replaced.

If the *Replace "#" with…* check box is selected, any series of # characters in the target
expression will be replaced by a number, starting with the number you enter.

You can resize the window any way you want and the controls will automatically
resize themselves to use all of the space.

## Regular Expressions

This documentation will be fairly brief and it assumes you have some knowledge of regular expressions.
You can learn more on [Microsoft's .NET Regular Expressions](https://msdn.microsoft.com/en-us/library/hs600312.aspx) page.

You use a regular expression to match a specific pattern of text, such as all words that begin
with "b" or all text starting with 0 and ending with a period.

### Regular Expression Syntax

| Pattern | |
| :-------------: | :----- |
`.` | Matches any single character.
`[ ]` | Indicates a character class. Matches any character inside the brackets. (For example, `[abc]` matches "a", "b", and "c")
`^` | If this metacharacter occurs at the start of a character class, it negates the character class. A negated character class matches any character except those inside the brackets. (For example, `[^abc]` matches all characters except "a", "b", and "c")<br><br>If `^` is at the beginning of the regular expression, it matches the beginning of the input. (For example, ^[abc] will only match input that begins with "a", "b", or "c")
`-` | In a character class, indicates a range of characters. (For example, `[0-9]` matches any of the digits "0" through "9")
`?` | Indicates that the preceding expression is optional: it matches once or not at all. (For example, `[0-9][0-9]?` matches "2" and "12")
`+` | Indicates that the preceding expression matches one or more times. (For example, [0-9]+ matches "1", "13", "666", and so on)
`*` | Indicates that the preceding expression matches zero or more times.
`??, +?, *?` | Non-greedy versions of `?`, `+`, and `*`. These match as little as possible, unlike the greedy versions which match as much as possible. Example: given the input "&lt;abc&gt;&lt;def&gt;", &lt;.*?&gt; matches "&lt;abc&gt;" while &lt;.*&gt; matches "&lt;abc&gt;&lt;def&gt;".
`{n}` | Match previous expression exactly n times.
`{n,}` | Match previous expression at least n times.
`{n,m}` | Match previous expression between n and m times.
`( )` | Grouping operator. The matching text can be referenced in the source expression with a backreference or in the target expression by a matching operator. Example: `(\d+,)*\d+` matches a list of numbers separated by commas (such as "1" or "1,23,456").
`\` | Escape character: interpret the next character literally (for example, `[0-9]+` matches one or more digits, but `[0-9]\+` matches a digit followed by a plus character). Also used for abbreviations (such as `\a` for any alphanumeric character; see table below).
`\#` | If `\` is followed by a number `n`, it matches the nth match group (starting from 0). Example: `&lt;{.*?}&gt;.*?&lt;/\0&gt;` matches "`&lt;head&gt;Contents&lt;/head&gt;`".
`$` | At the end of a regular expression, this character matches the end of the input. Example: `[0-9]$` matches a digit at the end of the input.
`|` | Alternation operator: separates two expressions, exactly one of which matches (for example, `T|the` matches "The" or "the").

### Abbreviations

You can also use abbreviations, such as `\d` instead of `[0-9]`.

| Pattern | |
| :-------------: | :----- |
`\d` | Any decimal digit
`\D` | Any non-decimal digit
`\w` | Any word character
`\W` | Any non-word character
`\s` | Any whitespace character
`\S` | Any non-whitespace character

## Tips

### Filename Conflicts

If you have a set of numbered files that you want to renumber, it's possible that the filenames might "collide" during renaming. For example:

| Current Name | New Name |
| :------------- | :----- |
test1.txt | test1.txt
test11.txt | test12.txt
test2.txt | test3.txt

If the source expression is **`test.+`** and the target expression is **`test#.txt`**,
NameTwister will display an error when it tries to rename **`test11.txt`** to **`test2.txt`**
because **`test2.txt`** already exists.

The workaround is to modify the rest of the filename too, not just the numbers.
In this example, you can change the target expression to **`test_#.txt`**. (Note the new underscore.)

### Numbers After Groups

You might have a set of numbered files and when you get more than nine, you want
to add zeroes so that the file names all have the same number of digits.

| Current Name | New Name |
| :------------- | :----- |
test1.txt | test01.txt
test2.txt | test02.txt
test3.txt | test03.txt

(In this example, it's easy to change `test` to `test0`, but if you have a more complicated file
name, you might need an expression to match the part before the number.)

If you want to add a zero (0) before a number, you might try this:

Source: `(t.+t)`

Target: `$10`

That won't work because "$10" looks like you want the tenth group (which doesn't exist).
The trick is to use braces around the group number.

Source: `(t.+t)`

Target: `${1}0`

Read more about [Substituting a Numbered Group](https://msdn.microsoft.com/en-us/library/ewy2t5e0.aspx#Numbered) on Microsoft.com.

#### Group Names

If you have a complex expression with many groups, it can be difficult to remember that $4 is the
year and $6 is the last name and $2 is the minute. Fortunately, you can name groups.

Filename: `20120526-1642 Camera 5 Studio backlot 1074a.png`

Source: `(?'yyyy'\d\d\d\d)(?'mm'\d\d)(?'dd'\d\d) (?'name'.+) (?'code'\d+a)(?'ext'\....)$`

Target: `${name} (Code ${code}) - ${dd}${mm}${yyyy}${'ext'}`

Read more about [Substitutions](https://msdn.microsoft.com/en-us/library/ewy2t5e0.aspx#Numbered) on Microsoft.com.

## Requirements

NameTwister runs on Microsoft<sup>®</sup> Windows<sup>®</sup> 11 and 10.
