using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collections.Pooled;
using UniversityBot.Core.DAL.Attachment;
using UniversityBot.EF.Extension;

namespace UniversityBot.EF.Repository
{
    public class AttachmentRepository
    {
        private AppDbContext _db;

        public AttachmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public void RemoveAll(IEnumerable<BotMessageAttachment> attachments)
        {
            using var entityForTrack = attachments.ToPooledDictionary(e => e.Id);
            using var files = entityForTrack.Values.Select(e => e.File).Where(e => e != null).ToPooledDictionary(e => e.Id);
            _db.RemoveByTrackInfo(_db.MessageAttachments, entityForTrack);
            _db.RemoveByTrackInfo(_db.Files, files);
        }

        public Task AddAll(IEnumerable<BotMessageAttachment> items)
        {
            return _db.MessageAttachments.AddRangeAsync(items);
        }
    }
}