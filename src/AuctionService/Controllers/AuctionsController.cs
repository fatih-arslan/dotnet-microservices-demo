using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item!.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound(); // 404
            }

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);

            // TODO: add current user as seller
            auction.Seller = "test";

            _context.Auctions.Add(auction);

            bool changesSaved = await _context.SaveChangesAsync() > 0;

            if (!changesSaved) return BadRequest("Could not save the changes to the DB"); // 400

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction)); // 201
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound(); //404

            // TODO: check seller == username

            auction.Item = _mapper.Map<Item>(updateAuctionDto);

            bool changesSaved = await _context.SaveChangesAsync() > 0;

            if (!changesSaved) return BadRequest("Could not save the changes to the DB"); // 400

            return Ok(); // 200
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            // TODO: check seller == username

            _context.Auctions.Remove(auction);

            bool changesSaved = _context.SaveChanges() > 0;

            if (!changesSaved) return BadRequest("Could not update db");

            return Ok();
        }
    }
}
