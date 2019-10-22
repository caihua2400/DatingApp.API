using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{user_id}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repository;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int user_id, int id)
        {
            if (user_id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _repository.GetMessage(id);

            if (messageFromRepo == null)
            {
                return NotFound();
            }

            return Ok(messageFromRepo);

        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int user_id, 
            [FromQuery]MessageParams messageParams)
        {
            if (user_id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageParams.UserId = user_id;

            var messagesFromRepo = await _repository.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);
            Response.AddPagination(messagesFromRepo.CurrentPage,messagesFromRepo.PageSize
            ,messagesFromRepo.TotalCount,messagesFromRepo.TotalPages);
            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessagesThread(int user_id, int recipientId)
        {
            if (user_id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messagesFromRepo = await _repository.GetMessageThread(user_id, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);
            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int user_id, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repository.GetUser(user_id);
            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageForCreationDto.SenderId = user_id;
            var recipient = await _repository.GetUser(messageForCreationDto.RecipientId);
            if (recipient == null)
            {
                return BadRequest("Could not find user");
            }

            var message = _mapper.Map<Message>(messageForCreationDto);
            _repository.Add(message);

            

            if (await _repository.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new {id = message.Id} , messageToReturn);
            }
            
            throw new Exception("creating message failed on save.");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int user_id, int id)
        {
            if (user_id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _repository.GetMessage(id);
            if (messageFromRepo.SenderId == user_id)
            {
                messageFromRepo.SenderDeleted = true;
            }

            if (messageFromRepo.RecipientId == user_id)
            {
                messageFromRepo.RecipientDeleted = true;
            }

            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
            {
                _repository.Delete(messageFromRepo);
            }

            if (await _repository.SaveAll())
            {
                return NoContent();
            }
            
            throw new Exception("error deleting the message.");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int user_id, int id)
        {
            if (user_id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var message = await _repository.GetMessage(id);
            if (message.RecipientId != user_id)
            {
                return Unauthorized();
            }

            message.IsRead = true;
            message.DateRead= DateTime.Now;
            await _repository.SaveAll();
            return NoContent();
        }
        
        
    }
}