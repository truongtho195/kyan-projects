using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Models
{
    public class LessonSoundModel 
    {
        public string LessonName { get; set; }
        public byte[] SoundFile { get; set; }
    }
}
