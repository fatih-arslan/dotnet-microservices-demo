using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    // Test methods are named accordng to the following convention: 
    // [name of the method being tested]_[scenario]_[expected result]

    [Fact]
    public void HasReservePrice_ReservePriceGtZero_True()
    {
        // arrange
        var auction = new Auction { Id = Guid.NewGuid(), ReservePrice = 10 };

        // act
        var result = auction.HasReservePrice();

        // assert
        Assert.True(result);
    }

    [Fact]
    public void HasReservePrice_ReservePriceIsZero_False()
    {
        // arrange
        var auction = new Auction { Id = Guid.NewGuid(), ReservePrice = 0 };

        // act
        var result = auction.HasReservePrice();

        // assert
        Assert.False(result);
    }

}
