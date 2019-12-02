using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cardofun.Core.ApiParameters;
using Cardofun.Core.Enumerables;
using Cardofun.Core.Enums;
using Cardofun.Core.Helpers;
using Cardofun.DataContext.Data;
using Cardofun.DataContext.Helpers;
using Cardofun.Domain.Models;
using Cardofun.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Cardofun.DataContext.Repositories
{
    public class CardofunRepository : ICardofunRepository
    {
        #region Fields
        /// <summary>
        /// Database context
        /// </summary>
        private readonly CardofunContext _context;
        #endregion Fields

        #region Constructor
        public CardofunRepository(CardofunContext context)
        {
            _context = context;
        }
        #endregion Constructor

        #region Functions
        /// <summary>
        /// Sets up includes, predicates and orderings
        /// </summary>
        /// <param name="requestSettings">Set of includes, orderings and other request settings</param>
        /// <param name="predicates">Set of predicates</param>
        /// <typeparam name="TEntity">Db entities for getting back</typeparam>
        /// <returns></returns>
        private IQueryable<TEntity> SetUpRequest<TEntity>(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> requestSettings = null, 
            params Expression<Func<TEntity, bool>> [] predicates) 
            where TEntity : class
        {
            var result = _context.Set<TEntity>().AsQueryable();
                        
            if(requestSettings != null)
                result = requestSettings(result);

            foreach (var predicate in predicates)
                result = result.Where(predicate);

            return result;
        }

        /// <summary>
        /// Gets an item with specified Id out of db context
        /// </summary>
        /// <param name="id">Key</param>
        /// <param name="requestSettings">Set of includes, orderings and other request settings</param>
        /// <typeparam name="TEntity">Type of entity to get</typeparam>
        /// <typeparam name="TKey">Type of the passing key (id) of entity to get</typeparam>
        /// <returns></returns>
        private async Task<TEntity> GetItemAsync<TEntity, TKey>(TKey id, Func<IQueryable<TEntity>, IQueryable<TEntity>> requestSettings = null) 
            where TEntity : class
        {
            // Getting the primary key info
            var keyProperty = _context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[0];

            var result = SetUpRequest(requestSettings: requestSettings);

            // Getting the requested entity joining along all the needed properties (tables)
            return await result.FirstOrDefaultAsync(e => EF.Property<TKey>(e, keyProperty.Name).Equals(id));
        }

        /// <summary>
        /// Gets an item with a given type and predicate out of db context
        /// </summary>
        /// <param name="requestSettings">Set of includes, orderings and other request settings</param>
        /// <param name="predicates">Conditions</param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
		private async Task<TEntity> GetItemByPredicatesAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> requestSettings = null, params Expression<Func<TEntity, bool>> [] predicates) 
            where TEntity : class
                => await SetUpRequest(requestSettings: requestSettings, predicates: predicates).FirstOrDefaultAsync();

        /// <summary>
        /// Gets all of items with a given type out of db context
        /// </summary>
        /// <param name="requestSettings">Set of includes, orderings and other request settings</param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
		private async Task<IEnumerable<TEntity>> GetAllItemsAsync<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> requestSettings = null) 
            where TEntity : class
                => await SetUpRequest(requestSettings: requestSettings).ToArrayAsync(); 

        /// <summary>
        /// Gets all of items with a given type out of db context
        /// </summary>
        /// <param name="requestSettings">Set of includes, orderings and other request settings</param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
		private async Task<PagedList<TEntity>> GetPageOfItemsAsync<TEntity>(PaginationParams paginationParams, 
            Func<IQueryable<TEntity>, IQueryable<TEntity>> requestSettings = null,
            params Expression<Func<TEntity, bool>> [] predicates) 
                where TEntity : class
                    => await SetUpRequest(requestSettings: requestSettings, predicates: predicates)
                        .ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);

        /// <summary>
        /// Gets items with a given type and predicate out of db context
        /// </summary>
        /// <param name="requestSettings">Set of includes, orderings and other request settings</param>
        /// <param name="predicates">Conditions</param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
		private async Task<IEnumerable<TEntity>> GetItemsByPredicates<TEntity>(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> requestSettings = null, params Expression<Func<TEntity, bool>> [] predicates) 
            where TEntity : class
                => await SetUpRequest(requestSettings: requestSettings, predicates: predicates).ToArrayAsync();
        #endregion Functions

        #region ICardofunRepository
        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        public void Add<TEntity>(TEntity entity)
            => _context.Add(entity);

        /// <summary>
        /// Deletes an entity from the repository
        /// </summary>
        public void Delete<TEntity>(TEntity entity)
            => _context.Remove(entity);

        /// <summary>
        /// Saves all changes being made previously
        /// </summary>
        /// <returns>true - changes saved</returns>
        public async Task<Boolean> SaveChangesAsync()
            => await _context.SaveChangesAsync() > 0;
        
        /// <summary>
        /// Starts transaction for the repository
        /// </summary>
        public void StartTransaction()
            => _context.Database.BeginTransaction();

        /// <summary>
        /// Commits transaction for the repository
        /// </summary>
        public void CommitTransaction()
            => _context.Database.CommitTransaction();

        #region Users
        /// <summary>
        /// Checks if user with the given login already exists 
        /// </summary>
        /// <param name="login">login by which to make a search</param>
        /// <returns></returns>
        public async Task<Boolean> CheckIfUserExists(String login)
            => await _context.Users.AnyAsync(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets a user out of the repository
        /// </summary>
        /// <param name="id">id of the user that ought to be returned</param>
        /// <returns></returns>
        public async Task<User> GetUserAsync(int id)
            => await GetItemAsync<User, Int32>(id, 
                user => user
                    .Include(u => u.City)
                        .ThenInclude(c => c.Country)
                    .Include(u => u.Photos)
                        .ThenInclude(p => p.Photo)
                    .Include(u => u.LanguagesTheUserLearns)
                        .ThenInclude(l => l.Language)
                    .Include(u => u.LanguagesTheUserSpeaks)
                        .ThenInclude(l => l.Language));          
        
        
        /// <summary>
        /// Sets up includes, predicates and orderings
        /// </summary>
        /// <param name="userParams"></param>
        /// <returns></returns>
        private IQueryable<User> SetUpUsersRequest(UserParams userParams)
        {
            var result = SetUpRequest<User>(
                // request settings
                requestSettings: 
                    user => user
                        .Include(x=>x.City)
                            .ThenInclude(x => x.Country) 
                        .Include(x => x.Photos) 
                            .ThenInclude(x => x.Photo)
                        .Include(x => x.IncomingFriendRequests)
                        .Include(x => x.OutcomingFriendRequests)
                        // Uncomment next lines if there's a need to include languages users speak and learn
                        // .Include(x => x.LanguagesTheUserLearns) 
                        //     .ThenInclude(x => x.Language) 
                        // .Include(x => x.LanguagesTheUserSpeaks) 
                        //     .ThenInclude(x => x.Language)

                        //  Orderings
                        .OrderByDescending(x => x.LastActive),
                // predicates
                user => user.Id != userParams.UserId,
                user => !userParams.Sex.HasValue || user.Sex == userParams.Sex,
                user => !userParams.AgeMin.HasValue || user.BirthDate.ToAges() >= userParams.AgeMin,
                user => !userParams.AgeMax.HasValue || user.BirthDate.ToAges() <= userParams.AgeMax,
                user => !userParams.CityId.HasValue || user.CityId == userParams.CityId,
                user => userParams.CityId.HasValue || String.IsNullOrEmpty(userParams.CountryIsoCode) || user.City.CountryIsoCode.Equals(userParams.CountryIsoCode),
                user => String.IsNullOrEmpty(userParams.LanguageLearningCode) || user.LanguagesTheUserLearns.Any(l => l.LanguageCode.Equals(userParams.LanguageLearningCode)),
                user => String.IsNullOrEmpty(userParams.LanguageSpeakingCode) || user.LanguagesTheUserSpeaks.Any(l => l.LanguageCode.Equals(userParams.LanguageSpeakingCode)));
            return result;
        }
        
        /// <summary>
        /// Gets page of users out of the repository
        /// </summary>
        /// <returns></returns>
        public async Task<PagedList<User>> GetPageOfUsersAsync(UserParams userParams)
            => await SetUpUsersRequest(userParams)
                .ToPagedListAsync(userParams.PageNumber, userParams.PageSize);
        
        /// <summary>
        /// Gets page of friends out of the repository
        /// </summary>
        /// <returns></returns>
        public async Task<PagedList<User>> GetPageOfFriendsAsync(UserFriendParams userFriendParams)
            => await SetUpUsersRequest(userFriendParams)
                // Appart of the previous settings we should set up that those users also shoud have
                // related friend requests with needed status
                .Where(user => (
                    user.OutcomingFriendRequests.Any(ifr => ifr.ToUserId == userFriendParams.UserId && userFriendParams.FriendshipStatus.Contains(ifr.Status))
                    ||
                    user.IncomingFriendRequests.Any(ofr => ofr.FromUserId == userFriendParams.UserId && userFriendParams.FriendshipStatus.Contains(ofr.Status))))
                .Where(user =>
                    // Get all friends regardless of who initiated firendship
                    userFriendParams.IsFriendshipOwned == null 
                    ||
                    // Get friendships initiated not by requested user  
                    (!userFriendParams.IsFriendshipOwned.Value && user.OutcomingFriendRequests.Any(ofr => ofr.ToUserId == userFriendParams.UserId))
                    ||
                    // Get friendships initiated by requested user
                    (userFriendParams.IsFriendshipOwned.Value && user.IncomingFriendRequests.Any(ofr => ofr.FromUserId == userFriendParams.UserId)))
                .ToPagedListAsync(userFriendParams.PageNumber, userFriendParams.PageSize);
        #endregion Users

        #region Languages
        /// <summary>
        /// Gets languages by given search pattern
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Language>> GetLanguagesAsync(String languageSearchPattern)
            => await GetItemsByPredicates<Language>(predicates: language => language.Name.ToUpper().Contains(languageSearchPattern.ToUpper()));
        #endregion Languages

        #region Countries
        /// <summary>
        /// Gets countries by given search pattern
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Country>> GetCountriesAsync(String countrySearchPattern)
            => await GetItemsByPredicates<Country>(
                predicates: country => country.Name.ToUpper().StartsWith(countrySearchPattern.ToUpper()));
        #endregion Countries

        #region Cities
        /// <summary>
        /// Gets cities by given search pattern
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<City>> GetCitiesAsync(String citySearchPattern)
            => await GetItemsByPredicates<City>(
                requestSettings: city => city.Include(c => c.Country),
                predicates: city => city.Name.ToUpper().StartsWith(citySearchPattern.ToUpper()));
        #endregion Cities
    
        #region Photos
        /// <summary> 
        /// Gets photo by given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UserPhoto> GetUserPhotoAsync(Guid id)
            => await GetItemAsync<UserPhoto, Guid>(id);

        /// <summary>
        /// Gets user's main photo
        /// </summary>
        /// <param name="userId">User of which main photo is needed</param>
        /// <returns></returns>
        public async Task<UserPhoto> GetMainPhotoForUserAsync(Int32 userId)
            => await GetItemByPredicatesAsync<UserPhoto>(null,
                photo => photo.UserId == userId,
                photo => photo.IsMain);
        #endregion Photos

        #region FriendshipRequests
        /// <summary>
        /// Gets a request for friendship
        /// </summary>
        /// <param name="fromUserId">Id of a user that sent the request</param>
        /// <param name="toUserId">Id of a user that received the request</param>
        /// <returns></returns>
        public async Task<FriendRequest> GetFriendRequestAsync(Int32 fromUserId, Int32 toUserId)
            => await _context.FriendRequests.FindAsync(fromUserId, toUserId);
        #endregion FriendshipRequests
    
        #region Messages
        /// <summary>
        /// Gets a message by it's Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Message> GetMessageAsync(Guid id)
            => await GetItemAsync<Message, Guid>(id, message => message.Include(m => m.Photo));

        /// <summary>
        /// Gets a page of lastly sent messages to/from a user 
        /// </summary>
        /// <returns></returns>
        public async Task<PagedList<Message>> GetLastMessagesForUser(MessagePrams messagePrams)
        {
            var request = SetUpRequest<Message>(message => message
                .Include(m => m.Photo)
                .Include(m => m.Sender)
                    .ThenInclude(s => s.Photos)
                        .ThenInclude(p => p.Photo)
                .Include(m => m.Recipient)
                    .ThenInclude(s => s.Photos)
                        .ThenInclude(p => p.Photo));

            // Getting top messages sent by different users
            if (messagePrams.Container == MessageContainer.Thread)
            {
                request = request
                    .GroupBy(m => new
                    {
                        MinId = m.SenderId <= m.RecipientId ? m.SenderId : m.RecipientId,
                        MaxId = m.SenderId > m.RecipientId ? m.SenderId : m.RecipientId
                    })
                    .Select(gm => gm.OrderByDescending(m => m.SentAt).FirstOrDefault())
                    .AsQueryable();
            }
            // Getting top messages sent by other users (not by the one who's requesting)
            // and only those that hasn't been read by the user yet
            if (messagePrams.Container == MessageContainer.Unread)
            {
                request = request
                    .GroupBy(m => m.SenderId)
                    .Select(gm => gm.OrderByDescending(m => m.SentAt).FirstOrDefault(m => !m.IsRead))
                    .Where(m => m != null)
                    .AsQueryable();           
            }
            
            return await request.OrderByDescending(m => m.SentAt)
                .Where(message =>
                    // Get messages sent to user and only the unread ones
                    messagePrams.Container != MessageContainer.Unread 
                    ||
                    message.RecipientId == messagePrams.UserId && !message.IsRead)
                    // Get all messages sent related to user
                .Where(message => 
                    messagePrams.Container != MessageContainer.Thread 
                    ||
                    (
                        message.RecipientId == messagePrams.UserId
                        ||
                        message.SenderId == messagePrams.UserId
                    ))
                .ToPagedListAsync(messagePrams.PageNumber, messagePrams.PageSize);
        }

        /// <summary>
        /// Gets a paginated message thread between two users
        /// </summary>
        /// <returns></returns>
        public async Task<PagedList<Message>> GetPaginatedMessageThread(MessageThreadPrams messageParams)
            => await GetPageOfItemsAsync<Message>(messageParams, 
                message => message
                    .Include(m => m.Sender)
                        .ThenInclude(r => r.Photos)
                            .ThenInclude(p => p.Photo)
                    .Include(m => m.Recipient)
                        .ThenInclude(r => r.Photos)
                            .ThenInclude(p => p.Photo)
                    .OrderByDescending(m => m.SentAt),
                message => 
                        (message.SenderId == messageParams.UserId
                        &&
                        message.RecipientId == messageParams.SecondUserId)
                    ||
                        (message.RecipientId == messageParams.UserId
                        &&
                        message.SenderId == messageParams.SecondUserId)
            );
        #endregion Messages

    }
    #endregion ICardofunRepository
}