using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common.MongoDB;
using Play.Inventory.Service.Dtos;
using Play.Iventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("inventory")]
    public class ItemsController : ControllerBase
    {
        private readonly IMongoRepository<InventoryItem> _itemsRepository;

        public ItemsController(IMongoRepository<InventoryItem> itemsRepository)
        {
            _itemsRepository = itemsRepository;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> Get(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var items = (await _itemsRepository.GetMany(i => i.UserId == userId))
                .Select(i => i.AsDto());

            return Ok(items);
        }

        [HttpPost]
        public async Task<ActionResult> Post(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await _itemsRepository.GetOne(
                item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await _itemsRepository.InsertOne(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await _itemsRepository.UpdateOne(item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId, inventoryItem);
            }

            return Ok();
        }
    }
}