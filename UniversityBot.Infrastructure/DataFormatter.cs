using System;
using Microsoft.Extensions.Logging;
using SmartFormat;
using UniversityBot.Core;

namespace UniversityBot.Infrastructure
{
    public sealed class DataFormatter
    {
        private readonly ILogger<DataFormatter> _logger;

        public DataFormatter(ILogger<DataFormatter> logger)
        {
            _logger = logger;
        }
        
        public string Format(string input, FormatRequest request)
        {
            try
            {
                return Smart.Format(input, request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, input);
                return input;
            }
        }
    }
}