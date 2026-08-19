using Xunit;
using Domain.Entities;
using Domain.Exceptions;

namespace Domain.Tests;

public class UserAddressTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_ValidInput_CreatesAddress()
    {
        var address = UserAddress.Create(UserId, "Home", "Street 1", "Tehran", "09121112233", "1234567890", isDefault: true);

        Assert.Equal(UserId, address.UserId);
        Assert.Equal("Home", address.Title);
        Assert.True(address.IsDefault);
    }

    [Fact]
    public void Create_EmptyUserId_Throws()
    {
        Assert.Throws<DomainException>(() => UserAddress.Create(Guid.Empty, "Home", "Street 1", null, "09121112233", null, false));
    }

    [Fact]
    public void Create_EmptyTitle_Throws()
    {
        Assert.Throws<DomainException>(() => UserAddress.Create(UserId, " ", "Street 1", null, "09121112233", null, false));
    }

    [Fact]
    public void Create_EmptyAddressLine_Throws()
    {
        Assert.Throws<DomainException>(() => UserAddress.Create(UserId, "Home", " ", null, "09121112233", null, false));
    }

    [Fact]
    public void Create_EmptyPhoneNumber_Throws()
    {
        Assert.Throws<DomainException>(() => UserAddress.Create(UserId, "Home", "Street 1", null, " ", null, false));
    }

    [Fact]
    public void Create_TitleTooLong_Throws()
    {
        Assert.Throws<DomainException>(() => UserAddress.Create(UserId, new string('a', 101), "Street 1", null, "09121112233", null, false));
    }

    [Fact]
    public void Update_ChangesFields()
    {
        var address = UserAddress.Create(UserId, "Home", "Street 1", "Tehran", "09121112233", null, false);

        address.Update("Office", "Street 2", "Karaj", "09351112233", "9876543210");

        Assert.Equal("Office", address.Title);
        Assert.Equal("Street 2", address.AddressLine);
        Assert.Equal("Karaj", address.City);
        Assert.Equal("09351112233", address.PhoneNumber);
        Assert.Equal("9876543210", address.PostalCode);
    }

    [Fact]
    public void SetAsDefault_And_ClearDefault_ToggleFlag()
    {
        var address = UserAddress.Create(UserId, "Home", "Street 1", null, "09121112233", null, false);

        address.SetAsDefault();
        Assert.True(address.IsDefault);

        address.ClearDefault();
        Assert.False(address.IsDefault);
    }
}
