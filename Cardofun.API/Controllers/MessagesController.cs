using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Cardofun.Domain.Models;
using Cardofun.Interfaces.DTOs;
using Cardofun.Interfaces.Repositories;
using Cardofun.Interfaces.ServiceProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cardofun.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        #region Fields
        private readonly ICardofunRepository _cardofunRepository;
        private readonly IMapper _mapper;
        private readonly IImageProvider _imageProvider;
        #endregion Fields
        
        #region Constructor
        public MessagesController(ICardofunRepository cardofunRepository, IMapper mapper, IImageProvider imageProvider)
        {
            _mapper = mapper;
            _cardofunRepository = cardofunRepository;
            _imageProvider = imageProvider;
        }
        #endregion Constructor

        #region MessagesController methods

        /// <summary>
        /// Gets a message by it's Id
        /// </summary>
        /// <param name="userId">Id of a user that is trying to get the message</param>
        /// <param name="id">Id of the message</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = nameof(GetMessage))]
        public async Task<IActionResult> GetMessage(Int32 userId, Guid id)
        {
            if (userId != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _cardofunRepository.GetMessageAsync(id);

            if (messageFromRepo == null)
                return NotFound();
           
            if (messageFromRepo.SenderId == userId || messageFromRepo.RecipientId == userId)
                return Ok(_mapper.Map<MessageForReturnDto>(messageFromRepo));
            else
                return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(Int32 userId, 
            [FromForm]MessageForCreationDto messageForCreation)
        {
            if (userId != Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreation.SenderId = userId;

            var recepient = await _cardofunRepository.GetUserAsync(messageForCreation.RecipientId);

            if (recepient == null)
                return BadRequest("Recepient hasn't been found");  

            if (messageForCreation.File != null)
                messageForCreation.Photo = await _imageProvider.SavePictureAsync(messageForCreation.File);

            var message = _mapper.Map<Message>(messageForCreation);
            _cardofunRepository.Add(message);

            if (!await _cardofunRepository.SaveChangesAsync())
                return BadRequest("Could not send the message");
            
            var messageToReturn = _mapper.Map<MessageForReturnDto>(message);
            return CreatedAtRoute(nameof(GetMessage), new { userId, id = messageToReturn.Id }, messageToReturn);
        }
        #endregion MessagesController methods
    }
}