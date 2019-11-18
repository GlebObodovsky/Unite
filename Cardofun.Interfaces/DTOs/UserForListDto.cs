using System;
using Cardofun.Core.Enums;

namespace Cardofun.Interfaces.DTOs
{
    public class UserForListDto
    {
        public Int32 Id { get; set; }
        public String Login { get; set; }
        public String Name { get; set; }
        public Int32 Age { get; set; }
        public Sex Sex { get; set; }
        public CityDto City { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        /// <summary>
        /// Url of the main user's photo
        /// </summary>
        /// <value></value>
        public String PhotoUrl { get; set; }
        /// <summary>
        /// Frienship status (related to a requested user)
        /// </summary>
        /// <value></value>
        public FriendshipRequestDto Friendship { get; set; }        


        // Uncomment next lines if there's a need to return languages users speak and learn

        /// <summary>
        /// Collection of languages the user speaks and want to help out with
        /// </summary>
        /// <value></value>
        // public IEnumerable<LanguageLevelDto> LanguagesTheUserSpeaks { get; set; }
        /// <summary>
        /// Collection of languages the user learns and seeks for helping with
        /// </summary>
        /// <value></value>
        // public IEnumerable<LanguageLevelDto> LanguagesTheUserLearns { get; set; }
    }
}