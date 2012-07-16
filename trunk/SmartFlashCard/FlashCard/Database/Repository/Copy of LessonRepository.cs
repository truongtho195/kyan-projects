using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.Database.Repository
{
    public class LessonRepository:UnitOfWork<Lesson>
    {
        #region Ctor
        public LessonRepository()
        {

        } 
        #endregion

        #region Extenion Methods
        public void SaveLessonWithRelation(LessonModel lessonModel)
        {
            if (lessonModel.IsNew)
            {
                
            }
        }
        #endregion

    }
}
