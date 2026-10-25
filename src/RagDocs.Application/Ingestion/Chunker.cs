using System.Text;
using System.Text.RegularExpressions;
using RagDocs.Application.Abstractions;

namespace RagDocs.Application.Ingestion;

public sealed partial class Chunker(ITokenCounter tokenCounter)
{
    public IReadOnlyList<TextChunk> Split(IReadOnlyList<ExtractedPage> pages, ChunkingOptions options)
    {
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxTokens <= 0 || options.OverlapTokens < 0 || options.OverlapTokens >= options.MaxTokens)
        {
            throw new ArgumentException("Need MaxTokens > 0 and 0 <= OverlapTokens < MaxTokens.", nameof(options));
        }

        var units = CreateUnits(pages, options.MaxTokens);

        var chunks = new List<TextChunk>();
        var current = new List<Unit>();
        var currentTokens = 0;
        var overlapCount = 0;   // how many units at the start of `current` are overlap

        foreach (var unit in units)
        {
            var hasNewContent = current.Count > overlapCount;

            if (unit.IsHeading && hasNewContent)
            {
                // New section: emit and start clean, without overlap.
                Emit(chunks, current, overlapCount);
                current.Clear();
                currentTokens = 0;
                overlapCount = 0;
            }
            else if (hasNewContent && currentTokens + unit.Tokens > options.MaxTokens)
            {
                // Full: emit, then carry the tail over as overlap.
                Emit(chunks, current, overlapCount);

                var overlap = TakeOverlap(current, options.OverlapTokens);
                current.Clear();
                current.AddRange(overlap);
                currentTokens = overlap.Sum(u => u.Tokens);
                overlapCount = overlap.Count;
            }

            // If overlap + this unit still doesn't fit, drop overlap from the front.
            while (overlapCount > 0 && currentTokens + unit.Tokens > options.MaxTokens)
            {
                currentTokens -= current[0].Tokens;
                current.RemoveAt(0);
                overlapCount--;
            }

            current.Add(unit);
            currentTokens += unit.Tokens;
        }

        if (current.Count > overlapCount)
        {
            Emit(chunks, current, overlapCount);
        }

        return chunks;
    }

    private void Emit(List<TextChunk> chunks, List<Unit> units, int overlapCount)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < units.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(units[i].StartsParagraph ? "\n\n" : " ");
            }

            builder.Append(units[i].Text);
        }

        var text = builder.ToString();
        var pageNumber = units[overlapCount].PageNumber;

        chunks.Add(new TextChunk(chunks.Count, text, pageNumber, tokenCounter.CountTokens(text)));
    }

    private static List<Unit> TakeOverlap(List<Unit> units, int overlapTokens)
    {
        var overlap = new List<Unit>();
        var tokens = 0;

        for (var i = units.Count - 1; i >= 0; i--)
        {
            if (tokens + units[i].Tokens > overlapTokens)
            {
                break;
            }

            overlap.Insert(0, units[i]);
            tokens += units[i].Tokens;
        }

        return overlap;
    }

    private List<Unit> CreateUnits(IReadOnlyList<ExtractedPage> pages, int maxTokens)
    {
        var units = new List<Unit>();

        foreach (var page in pages)
        {
            foreach (var rawParagraph in ParagraphBreak().Split(page.Text))
            {
                // Inside a paragraph, line wraps and repeated spaces become single spaces.
                var paragraph = Whitespace().Replace(rawParagraph, " ").Trim();

                if (paragraph.Length == 0)
                {
                    continue;
                }

                var isHeading = paragraph.StartsWith('#');

                var sentences = isHeading
                    ? new[] { paragraph }
                    : SentenceEnd().Split(paragraph).Where(s => s.Length > 0).ToArray();

                var isFirst = true;

                foreach (var sentence in sentences)
                {
                    foreach (var piece in SplitOversized(sentence, maxTokens))
                    {
                        units.Add(new Unit(
                            piece,
                            page.PageNumber,
                            tokenCounter.CountTokens(piece),
                            StartsParagraph: isFirst,
                            IsHeading: isHeading && isFirst));

                        isFirst = false;
                    }
                }
            }
        }

        return units;
    }

    // A single sentence longer than MaxTokens is split by words (rare: tables, lists, bad PDFs).
    private List<string> SplitOversized(string text, int maxTokens)
    {
        if (tokenCounter.CountTokens(text) <= maxTokens)
        {
            return [text];
        }

        var pieces = new List<string>();
        var builder = new StringBuilder();

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = builder.Length == 0 ? word : $"{builder} {word}";

            if (builder.Length > 0 && tokenCounter.CountTokens(candidate) > maxTokens)
            {
                pieces.Add(builder.ToString());
                builder.Clear().Append(word);
            }
            else
            {
                builder.Clear().Append(candidate);
            }
        }

        if (builder.Length > 0)
        {
            pieces.Add(builder.ToString());
        }

        return pieces;
    }

    [GeneratedRegex(@"\n\s*\n")]
    private static partial Regex ParagraphBreak();

    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex SentenceEnd();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private sealed record Unit(string Text, int? PageNumber, int Tokens, bool StartsParagraph, bool IsHeading);
}