using System;
using DeepMorphy;
using DeepMorphy.Model;
using Microsoft.Extensions.Logging;

namespace UniversityBot.Infrastructure.WordProcessing
{
    public sealed class WordLemmatizer
    {
        private readonly MorphAnalyzer _morphAnalyzer;
        private readonly Tag _tag;
        private ILogger<WordLemmatizer> _logger;

        public WordLemmatizer(ILogger<WordLemmatizer> logger)
        {
            _logger = logger;
            
            try
            {
                _morphAnalyzer = new MorphAnalyzer(withLemmatization: true);
                _tag = _morphAnalyzer.TagHelper.CreateTag("прил", gndr: "жен", nmbr: "ед", @case: "им");
            }
            catch (Exception e)
            {
                // ignore
                _morphAnalyzer = null;
                _tag = null;
                
                _logger.LogError(e, "Create MorphAnalyzer");
            }
        }

        public string Lemmatize(string word)
        {
            try
            {
                return _morphAnalyzer?.Lemmatize(word, _tag) ?? word;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Lemmatize '{0}'", word);
                return word;
            }
        }
    }
}