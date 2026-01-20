using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MusicTheoryTrainerBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();

// using = import a namespace; System = contains Console, Random, Array, Math, etc.
// Gives access to List<T>, HashSet<T>, etc.

// Gives LINQ methods like Select(), Where(), ToArray()

internal abstract partial class Program                       // class Program = container for all variables + methods in this console app
{
    static readonly Random Rng = new(); // static = shared; readonly = cannot be reassigned; Random = type; new() creates the Random object once

    static readonly string[] Notes = // string[] = array of strings; Notes holds our 12 chromatic notes (sharp spelling)
    [
        "C","C#","D","D#","E","F","F#","G","G#","A","A#","B"
    ];

    static readonly (string Name, int Semitones)[] Intervals = // tuple array: each interval has a Name and Semitone count
    [
        ("m2", 1), ("M2", 2), ("m3", 3), ("M3", 4),
        ("P4", 5), ("TT", 6), ("P5", 7),
        ("m6", 8), ("M6", 9), ("m7", 10), ("M7", 11),
        ("P8", 12)
    ];

    static readonly (int Degree, string Name, string Shorthand)[] DegreeNames = // tuple array: degree number + label + shorthand
    [
        (1, "Tonic", "T"),
        (2, "Supertonic", "ST"),
        (3, "Mediant", "M"),
        (4, "Subdominant", "SD"),
        (5, "Dominant", "D"),
        (6, "Submediant", "SM"),
        (7, "Leading tone", "LT")
    ];

    static readonly int[] MajorScaleSteps = [0, 2, 4, 5, 7, 9, 11]; // semitone offsets for a major scale from the root

    private static int _correct;    // total correct answers across the whole program
    private static int _total = 0;      // total questions attempted across the whole program
    private static int _streak = 0;     // current correct-in-a-row streak
    private static int _bestStreak; // best streak achieved

    private static void Main() // Main() = where the program starts
    {
        Console.Title = "Music Theory Trainer"; // set console window title

        ShowIntro(); // print instructions

        while (true) // menu loop (keeps going until user chooses Exit)
        {
            Console.WriteLine();
            Console.WriteLine("=== MENU ===");
            Console.WriteLine("1) Arcade Mode (mixed questions, lives, levels)");
            Console.WriteLine("2) Interval Quiz (single question)");
            Console.WriteLine("3) Scale Degree Quiz (single question)");
            Console.WriteLine("4) Chord Quiz (single question)");
            Console.WriteLine("5) Stats");
            Console.WriteLine("0) Exit");
            Console.Write("Choose: ");

            var choice = Console.ReadLine()?.Trim(); // read menu choice

            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    ArcadeMode();
                    break;

                case "2":
                    IntervalQuizRound();
                    break;

                case "3":
                    ScaleDegreeQuizRound();
                    break;

                case "4":
                    ChordQuizRound();
                    break;

                case "5":
                    ShowStats();
                    break;

                case "0":
                    Console.WriteLine("Later ✌️");
                    return;

                default:
                    Console.WriteLine("Pick 0–5.");
                    break;
            }
        }
    }

    private static void ShowIntro() // prints opening instructions
    {
        Console.WriteLine("🎵 MUSIC THEORY TRAINER");
        Console.WriteLine("Type answers like: C#, F#, A");
        Console.WriteLine("For chord tones, type notes separated by spaces, e.g.: C E G");
        Console.WriteLine("No flats in this version (yet) — we’ll add flats after you test it.");
        Console.WriteLine();
    }

    // -------------------------
    // ARCADE MODE (LONGER GAME)
    // -------------------------
    private static void ArcadeMode() // runs a long “game session” with lives + levels
    {
        var lives = 3;           // how many mistakes you can make
        var level = 1;           // difficulty level
        var correctThisRun = 0;  // score for just this arcade session

        Console.WriteLine("🔥 ARCADE MODE");
        Console.WriteLine("You have 3 lives. Get questions right to level up.");
        Console.WriteLine("Type 'exit' anytime to stop Arcade Mode.");
        Console.WriteLine();

        while (lives > 0) // keep asking questions until lives run out or user exits
        {
            Console.WriteLine($"❤ Lives: {lives} |  Level: {level} |  Run Score: {correctThisRun}");
            Console.WriteLine("--------------------------------------");

            int mode = Rng.Next(3); // random quiz type: 0 interval, 1 degree, 2 chord

            var gotItRight = mode switch // switch expression returns a value; here it returns true/false
            {
                0 => IntervalArcadeQuestion(level),
                1 => ScaleDegreeArcadeQuestion(),
                _ => ChordArcadeQuestion(level)
            };

            if (gotItRight) // if correct, update scores + streak
            {
                _correct++;
                _total++;
                _streak++;
                correctThisRun++;
                _bestStreak = Math.Max(_bestStreak, _streak);

                Console.WriteLine(" Nice!");
            }
            else // if wrong, lose a life and reset streak
            {
                _total++;
                _streak = 0;
                lives--;

                Console.WriteLine(" Oof.");
            }

            if (correctThisRun > 0 && correctThisRun % 5 == 0) // every 5 correct answers, level up
            {
                level++;
                Console.WriteLine($"⬆️ LEVEL UP! You are now Level {level}!");
            }

            Console.WriteLine();
            Console.Write("Press Enter for next question (or type exit): ");
            var next = Console.ReadLine();

            if (next != null && next.Trim().ToLower() == "exit") // allow exit command
                break;

            Console.WriteLine();
        }

        if (lives == 0) // game over message if you ran out of lives
        {
            Console.WriteLine("💀 GAME OVER! You ran out of lives.");
        }

        Console.WriteLine($"Final Run Score: {correctThisRun}");
        Console.WriteLine("Returning to menu...");
    }

    private static bool IntervalArcadeQuestion(int level) // returns true if user was correct
    {
        var start = Notes[Rng.Next(Notes.Length)];

        var possible = level >= 3
            ? Intervals
            : Intervals.Where(i => i.Semitones <= 7).ToArray(); // easier early levels: up to P5

        var interval = possible[Rng.Next(possible.Length)];
        var answer = Transpose(start, interval.Semitones);

        Console.WriteLine($"INTERVAL: {start} + {interval.Name} = ?");
        Console.Write("> ");
        var user = NormalizeNote(Console.ReadLine());

        return user == answer;
    }

    private static bool ScaleDegreeArcadeQuestion() // level not used yet, but kept for future difficulty upgrades
    {
        var key = Notes[Rng.Next(Notes.Length)];
        var degree = DegreeNames[Rng.Next(DegreeNames.Length)];

        var scale = BuildMajorScale(key);
        var answer = scale[degree.Degree - 1];

        Console.WriteLine($"SCALE DEGREE: In {key} major, what is Degree {degree.Degree} ({degree.Shorthand})?");
        Console.Write("> ");
        var user = NormalizeNote(Console.ReadLine());

        return user == answer;
    }

    static bool ChordArcadeQuestion(int level) // chord difficulty increases with level
    {
        var root = Notes[Rng.Next(Notes.Length)];

        var qualities = new List<(string Name, int[] Steps)>
        {
            ("maj", [0, 4, 7]),
            ("min", [0, 3, 7]),
        };

        if (level >= 2)
            qualities.Add(("dim", [0, 3, 6]));

        if (level >= 3)
            qualities.Add(("aug", [0, 4, 8]));

        var q = qualities[Rng.Next(qualities.Count)];
        var tones = q.Steps.Select(st => Transpose(root, st)).ToArray();

        Console.WriteLine($"CHORD: Spell {root} {q.Name} (3 notes, any order)");
        Console.Write("> ");
        var userTones = NormalizeChord(Console.ReadLine() ?? "");

        return SameSet(userTones, tones);
    }

    // -------------------------
    // SINGLE-QUESTION MODES
    // -------------------------
    static void IntervalQuizRound() // one interval question, then back to menu
    {
        var start = Notes[Rng.Next(Notes.Length)];
        var interval = Intervals[Rng.Next(Intervals.Length)];
        var answer = Transpose(start, interval.Semitones);

        Console.WriteLine("INTERVAL QUIZ:");
        Console.WriteLine($"What is {start} + {interval.Name} ?");
        Console.Write("> ");

        var user = NormalizeNote(Console.ReadLine());

        GradeSingleNote(user, answer, $"Correct: {start} + {interval.Name} = {answer}");
    }

    static void ScaleDegreeQuizRound() // one scale degree question, then back to menu
    {
        var key = Notes[Rng.Next(Notes.Length)];
        var degree = DegreeNames[Rng.Next(DegreeNames.Length)];

        var scale = BuildMajorScale(key);
        var answer = scale[degree.Degree - 1];

        Console.WriteLine("SCALE DEGREE QUIZ (MAJOR):");
        Console.WriteLine($"Key: {key} major");
        Console.WriteLine($"What note is Degree {degree.Degree} ({degree.Name}, {degree.Shorthand})?");
        Console.Write("> ");

        var user = NormalizeNote(Console.ReadLine());

        GradeSingleNote(user, answer, $"Correct: In {key} major, Degree {degree.Degree} is {answer}");
    }

    private static void ChordQuizRound() // one chord question, then back to menu
    {
        var root = Notes[Rng.Next(Notes.Length)];

        var qualities = new (string Name, int[] Steps)[]
        {
            ("maj", [0, 4, 7]),
            ("min", [0, 3, 7]),
            ("dim", [0, 3, 6]),
            ("aug", [0, 4, 8])
        };

        var q = qualities[Rng.Next(qualities.Length)];
        var tones = q.Steps.Select(st => Transpose(root, st)).ToArray();

        Console.WriteLine("CHORD QUIZ (TRIADS):");
        Console.WriteLine($"Spell the chord: {root} {q.Name}");
        Console.WriteLine("Type 3 notes separated by spaces (order doesn’t matter). Example: C E G");
        Console.Write("> ");

        var userInput = Console.ReadLine() ?? "";
        var userTones = NormalizeChord(userInput);

        _total++;

        var ok = SameSet(userTones, tones);

        if (ok)
        {
            _correct++;
            _streak++;
            _bestStreak = Math.Max(_bestStreak, _streak);
            Console.WriteLine($"✅ Correct. {root} {q.Name} = {string.Join(" ", tones)}");
        }
        else
        {
            _streak = 0;
            Console.WriteLine($"❌ Not quite. {root} {q.Name} = {string.Join(" ", tones)}");
            Console.WriteLine($"You typed: {string.Join(" ", userTones)}");
        }

        ShowQuickScore();
    }

    // -------------------------
    // GRADING + STATS
    // -------------------------
    static void GradeSingleNote(string? user, string answer, string explanation) // grades an answer that is a single note
    {
        _total++;

        if (!string.IsNullOrWhiteSpace(user) && user == answer)
        {
            _correct++;
            _streak++;
            _bestStreak = Math.Max(_bestStreak, _streak);
            Console.WriteLine(" Correct.");
        }
        else
        {
            _streak = 0;
            Console.WriteLine($" Wrong. {explanation}");

            Console.WriteLine(string.IsNullOrWhiteSpace(user) ? "You entered nothing." : $"You entered: {user}");
        }

        ShowQuickScore();
    }

    static void ShowQuickScore() // prints a single-line score summary
    {
        Console.WriteLine($"Score: {_correct}/{_total} | Streak: {_streak} | Best: {_bestStreak}");
    }

    static void ShowStats() // shows overall stats across all modes
    {
        Console.WriteLine("=== STATS ===");
        Console.WriteLine($"Correct: {_correct}");
        Console.WriteLine($"Total:   {_total}");
        Console.WriteLine($"Accuracy:{(_total == 0 ? 0 : (100.0 * _correct / _total)):0.0}%");
        Console.WriteLine($"Best streak: {_bestStreak}");
    }

    // -------------------------
    // MUSIC LOGIC HELPERS
    // -------------------------
    static string Transpose(string start, int semitones) // moves a note forward by semitones in our Notes[] list
    {
        int i = Array.IndexOf(Notes, start);

        if (i < 0) return start;

        int moved = i + semitones;

        moved %= Notes.Length;

        if (moved < 0) moved += Notes.Length;

        return Notes[moved];
    }

    static string[] BuildMajorScale(string key) // returns 7-note major scale built from semitone offsets
    {
        var rootIndex = Array.IndexOf(Notes, key);

        if (rootIndex < 0) return [key];

        var scale = new string[7];

        for (int i = 0; i < 7; i++)
        {
            int noteIndex = (rootIndex + MajorScaleSteps[i]) % 12;
            scale[i] = Notes[noteIndex];
        }

        return scale;
    }

    // -------------------------
    // INPUT NORMALIZATION
    // -------------------------
    static string? NormalizeNote(string? input) // cleans user input into our note format like "C" or "F#"
    {
        if (input == null) return null;

        var s = input.Trim().ToUpperInvariant()
            .Replace(" ", "")
            .Replace("♯", "#")
            .Replace("SHARP", "#")
            .Replace("B", "B");

        if (s.Length == 0) return "";

        s = s.Replace("♭", "B");

        if (s is [_, '#', ..])
            s = s[0] + "#";
        else
            s = s[0].ToString();

        return s;
    }

    static string[] NormalizeChord(string input) // splits a chord string like "C E G" into normalized notes
    {
        var parts = input
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeNote)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!)
            .ToArray();

        return parts;
    }

    static bool SameSet(string[] user, string[] answer) // compares two note lists ignoring order and duplicates
    {
        var a = new HashSet<string>(answer);
        var u = new HashSet<string>(user);

        return a.SetEquals(u);
    }
}
