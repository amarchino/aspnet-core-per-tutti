using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.Exceptions.Infrastructure
{
    public class ImagePersistenceException : Exception
    {
        public ImagePersistenceException(Exception innerException) : base("Couldn't persist the image", innerException)
        {}
    }
}
