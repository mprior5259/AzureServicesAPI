namespace AzureServicesAPI.Helpers
{
    public static class ContentTypeHelper
    {
        public static readonly HashSet<string> ValidContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/svg+xml",
            "image/bmp",
            "image/tiff",

            // Documents
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",

            // Text
            "text/plain",
            "text/html",
            "text/css",
            "text/csv",
            "text/xml",

            // Data
            "application/json",
            "application/xml",

            // Audio
            "audio/mpeg",
            "audio/wav",
            "audio/ogg",

            // Video
            "video/mp4",
            "video/mpeg",
            "video/ogg",
            "video/webm",

            // Archives
            "application/zip",
            "application/x-tar",
            "application/gzip",

            // Binary
            "application/octet-stream"
        };

        public static bool IsValid(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return false;

            return ValidContentTypes.Contains(contentType);
        }

        public static string SanitizeContentType(string? contentType)
        {
            if (IsValid(contentType))
                return contentType!;

            return "application/octet-stream";
        }
    }
}