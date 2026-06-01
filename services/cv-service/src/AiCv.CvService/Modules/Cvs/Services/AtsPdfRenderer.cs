using System.Globalization;
using System.Text;
using AiCv.CvService.Modules.Cvs.DTOs;

namespace AiCv.CvService.Modules.Cvs.Services;

public static class AtsPdfRenderer
{
    private const double PageWidth = 595.28;
    private const double PageHeight = 841.89;
    private const double MarginX = 48;
    private const double MarginTop = 48;
    private const double MarginBottom = 42;
    private const double ContentWidth = PageWidth - (MarginX * 2);

    public static byte[] Render(GeneratedCvResponseDto cv)
    {
        var canvas = new PdfCanvas();

        canvas.BeginPage();
        DrawHeader(canvas, cv);
        DrawSection(canvas, "Professional Summary");
        canvas.Paragraph(cv.Content.Summary, 10.8, 13.5);

        DrawEducation(canvas, cv.Content.Education);
        DrawProjects(canvas, cv.Content.Projects);
        DrawExperience(canvas, cv.Content.Experience);
        DrawSkills(canvas, cv.Content);
        DrawSimpleItems(canvas, "Certifications", cv.Content.Certifications);
        DrawSimpleItems(canvas, "Languages", cv.Content.Languages);

        return canvas.Build();
    }

    private static void DrawHeader(PdfCanvas canvas, GeneratedCvResponseDto cv)
    {
        var leftX = MarginX;
        var rightX = PageWidth - MarginX;

        canvas.Text(leftX, canvas.Y, cv.Content.Header.Name, 20, "F2");
        canvas.MoveDown(20);
        canvas.Text(leftX, canvas.Y, cv.Content.Header.Title, 11.5, "F2");
        canvas.MoveDown(14);
        canvas.Text(leftX, canvas.Y, $"Target role: {cv.Target.JobTitle} at {cv.Target.CompanyName}", 10, "F3");

        var contactLines = new[]
        {
            cv.Content.Header.Phone,
            cv.Content.Header.Email,
            cv.Content.Header.Address
        }.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();

        var contactY = PageHeight - MarginTop - 3;
        foreach (var line in contactLines)
        {
            canvas.RightText(rightX, contactY, line, 10, "F1");
            contactY -= 13;
        }

        canvas.MoveDown(15);
        canvas.Line(MarginX, canvas.Y, PageWidth - MarginX, canvas.Y, 0.9);
        canvas.MoveDown(14);
    }

    private static void DrawEducation(PdfCanvas canvas, List<GeneratedCvEducationDto> education)
    {
        if (education.Count == 0)
        {
            return;
        }

        DrawSection(canvas, "Education");
        foreach (var item in education)
        {
            var title = string.Join(" in ", new[] { item.Degree, item.Field }.Where(value => !string.IsNullOrWhiteSpace(value)));
            canvas.Text(MarginX, canvas.Y, title, 10.8, "F2");
            canvas.MoveDown(13);
            canvas.Text(MarginX + 8, canvas.Y, item.School, 10, "F3");
            canvas.MoveDown(12);
        }
    }

    private static void DrawProjects(PdfCanvas canvas, List<GeneratedCvProjectDto> projects)
    {
        if (projects.Count == 0)
        {
            return;
        }

        DrawSection(canvas, "Projects");
        foreach (var project in projects)
        {
            canvas.Text(MarginX, canvas.Y, project.Name, 10.8, "F2");
            canvas.MoveDown(13);
            if (!string.IsNullOrWhiteSpace(project.Description))
            {
                canvas.Paragraph(project.Description, 10, 12.5, MarginX + 8, ContentWidth - 8, "F3");
            }

            foreach (var bullet in project.Bullets.Take(2))
            {
                canvas.Bullet(bullet, 10);
            }

            if (!string.IsNullOrWhiteSpace(project.Technologies))
            {
                canvas.Paragraph($"Technologies: {project.Technologies}", 10, 12.5, MarginX + 8, ContentWidth - 8, "F1");
            }

            canvas.MoveDown(3);
        }
    }

    private static void DrawExperience(PdfCanvas canvas, List<GeneratedCvExperienceDto> experiences)
    {
        if (experiences.Count == 0)
        {
            return;
        }

        DrawSection(canvas, "Experience");
        foreach (var experience in experiences)
        {
            canvas.Text(MarginX, canvas.Y, $"{experience.Role} - {experience.Company}", 10.8, "F2");
            canvas.RightText(PageWidth - MarginX, canvas.Y, experience.Period, 10, "F3");
            canvas.MoveDown(13);

            if (!string.IsNullOrWhiteSpace(experience.Location))
            {
                canvas.Text(MarginX + 8, canvas.Y, experience.Location, 10, "F3");
                canvas.MoveDown(12);
            }

            foreach (var bullet in experience.Bullets)
            {
                canvas.Bullet(bullet, 10);
            }

            canvas.MoveDown(3);
        }
    }

    private static void DrawSkills(PdfCanvas canvas, GeneratedCvContentDto content)
    {
        var groups = content.SkillGroups.Count > 0
            ? content.SkillGroups
            : [new GeneratedCvSkillGroupDto { Label = "Core Skills", Items = content.Skills }];

        if (groups.Count == 0 || groups.All(group => group.Items.Count == 0))
        {
            return;
        }

        DrawSection(canvas, "Technical Skills");
        foreach (var group in groups.Where(group => group.Items.Count > 0))
        {
            canvas.Paragraph($"{group.Label}: {string.Join(", ", group.Items)}", 10.2, 12.5, MarginX, ContentWidth, "F1");
        }
    }

    private static void DrawSimpleItems(PdfCanvas canvas, string title, List<GeneratedCvSimpleItemDto> items)
    {
        var values = items
            .Where(item => !string.IsNullOrWhiteSpace(item.Name))
            .Select(item => string.IsNullOrWhiteSpace(item.Detail) ? item.Name : $"{item.Name} ({item.Detail})")
            .ToList();

        if (values.Count == 0)
        {
            return;
        }

        DrawSection(canvas, title);
        canvas.Paragraph(string.Join(", ", values), 10.2, 12.5);
    }

    private static void DrawSection(PdfCanvas canvas, string title)
    {
        canvas.EnsureSpace(42);
        canvas.MoveDown(5);
        canvas.Text(MarginX, canvas.Y, title.ToUpperInvariant(), 12, "F2");
        canvas.MoveDown(4);
        canvas.Line(MarginX, canvas.Y, PageWidth - MarginX, canvas.Y, 0.7);
        canvas.MoveDown(12);
    }

    private sealed class PdfCanvas
    {
        private readonly List<StringBuilder> _pages = [];
        private StringBuilder _current = new();

        public double Y { get; private set; } = PageHeight - MarginTop;

        public void BeginPage()
        {
            if (_current.Length > 0)
            {
                _pages.Add(_current);
            }

            _current = new StringBuilder();
            Y = PageHeight - MarginTop;
        }

        public void EnsureSpace(double needed)
        {
            if (Y - needed < MarginBottom)
            {
                BeginPage();
            }
        }

        public void MoveDown(double amount)
        {
            Y -= amount;
            if (Y < MarginBottom)
            {
                BeginPage();
            }
        }

        public void Text(double x, double y, string value, double size, string font)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            _current.AppendLine($"BT /{font} {Format(size)} Tf {Format(x)} {Format(y)} Td {PdfString(value)} Tj ET");
        }

        public void RightText(double rightX, double y, string value, double size, string font)
        {
            var textWidth = EstimateWidth(value, size);
            Text(rightX - textWidth, y, value, size, font);
        }

        public void Paragraph(string value, double size, double leading, double x = MarginX, double width = ContentWidth, string font = "F1")
        {
            foreach (var line in Wrap(value, size, width))
            {
                EnsureSpace(leading + 2);
                Text(x, Y, line, size, font);
                MoveDown(leading);
            }
        }

        public void Bullet(string value, double size)
        {
            var bulletWidth = 12;
            foreach (var line in Wrap(value, size, ContentWidth - 24))
            {
                EnsureSpace(14);
                Text(MarginX + 10, Y, "-", size, "F1");
                Text(MarginX + 10 + bulletWidth, Y, line, size, "F1");
                MoveDown(12.5);
            }
        }

        public void Line(double x1, double y1, double x2, double y2, double width)
        {
            _current.AppendLine($"{Format(width)} w {Format(x1)} {Format(y1)} m {Format(x2)} {Format(y2)} l S");
        }

        public byte[] Build()
        {
            if (_current.Length > 0)
            {
                _pages.Add(_current);
            }

            return PdfDocument.Build(_pages);
        }

        private static IEnumerable<string> Wrap(string value, double size, double width)
        {
            var words = NormalizeText(value).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var line = new StringBuilder();

            foreach (var word in words)
            {
                var candidate = line.Length == 0 ? word : $"{line} {word}";
                if (EstimateWidth(candidate, size) <= width || line.Length == 0)
                {
                    line.Clear();
                    line.Append(candidate);
                    continue;
                }

                yield return line.ToString();
                line.Clear();
                line.Append(word);
            }

            if (line.Length > 0)
            {
                yield return line.ToString();
            }
        }

        private static double EstimateWidth(string value, double size)
        {
            return NormalizeText(value).Length * size * 0.48;
        }

        private static string PdfString(string value)
        {
            var safe = NormalizeText(value)
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal);

            return $"({safe})";
        }

        private static string NormalizeText(string value)
        {
            var normalized = (value ?? string.Empty).Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(character <= 126 ? character : ' ');
            }

            return string.Join(" ", builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static string Format(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }

    private static class PdfDocument
    {
        public static byte[] Build(List<StringBuilder> pages)
        {
            var objects = new List<byte[]>();

            AddObject(objects, "<< /Type /Catalog /Pages 2 0 R >>");

            var pageRefs = Enumerable.Range(0, pages.Count)
                .Select(index => $"{6 + (index * 2)} 0 R");
            AddObject(objects, $"<< /Type /Pages /Kids [{string.Join(" ", pageRefs)}] /Count {pages.Count} >>");
            AddObject(objects, "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Roman >>");
            AddObject(objects, "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Bold >>");
            AddObject(objects, "<< /Type /Font /Subtype /Type1 /BaseFont /Times-Italic >>");

            for (var index = 0; index < pages.Count; index++)
            {
                var pageObjectNumber = 6 + (index * 2);
                var contentObjectNumber = pageObjectNumber + 1;
                AddObject(objects, $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {Format(PageWidth)} {Format(PageHeight)}] /Resources << /Font << /F1 3 0 R /F2 4 0 R /F3 5 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
                AddStreamObject(objects, pages[index].ToString());
            }

            return WritePdf(objects);
        }

        private static string Format(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static void AddObject(List<byte[]> objects, string value)
        {
            objects.Add(Encoding.ASCII.GetBytes(value));
        }

        private static void AddStreamObject(List<byte[]> objects, string stream)
        {
            var streamBytes = Encoding.ASCII.GetBytes(stream);
            var header = Encoding.ASCII.GetBytes($"<< /Length {streamBytes.Length} >>\nstream\n");
            var footer = Encoding.ASCII.GetBytes("endstream");
            objects.Add(Combine(header, streamBytes, footer));
        }

        private static byte[] WritePdf(List<byte[]> objects)
        {
            using var output = new MemoryStream();
            WriteAscii(output, "%PDF-1.4\n");

            var offsets = new List<long> { 0 };
            for (var index = 0; index < objects.Count; index++)
            {
                offsets.Add(output.Position);
                WriteAscii(output, $"{index + 1} 0 obj\n");
                output.Write(objects[index], 0, objects[index].Length);
                WriteAscii(output, "\nendobj\n");
            }

            var xrefOffset = output.Position;
            WriteAscii(output, $"xref\n0 {objects.Count + 1}\n");
            WriteAscii(output, "0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1))
            {
                WriteAscii(output, $"{offset:0000000000} 00000 n \n");
            }

            WriteAscii(output, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
            return output.ToArray();
        }

        private static byte[] Combine(params byte[][] chunks)
        {
            var result = new byte[chunks.Sum(chunk => chunk.Length)];
            var offset = 0;
            foreach (var chunk in chunks)
            {
                Buffer.BlockCopy(chunk, 0, result, offset, chunk.Length);
                offset += chunk.Length;
            }

            return result;
        }

        private static void WriteAscii(Stream stream, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
