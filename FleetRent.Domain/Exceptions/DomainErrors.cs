namespace FleetRent.Domain.Exceptions;

public static class DomainErrors
{
    public static class General
    {
        public static readonly DomainError Unexpected = new("An unexpected domain error has occurred.");
    }

    public static class Driver
    {
        public static readonly DomainError CnpjAlreadyExists = new("CNPJ already exists.");
        public static readonly DomainError LicenseNumberAlreadyExists = new("License number already exists.");
        public static readonly DomainError NotFound = new("Driver not found.");
        public static readonly DomainError NotCategoryA = new("Driver must be enabled for category A.");
        public static readonly DomainError InvalidData = new("All driver information is required.");
    }

    public static class Bike
    {
        public static readonly DomainError PlateAlreadyExists = new("Plate already exists.");
        public static readonly DomainError PlateRequired = new("Plate is required.");
        public static readonly DomainError NotFound = new("Bike not found.");
        public static readonly DomainError CannotRemoveWithRentals = new("Bike cannot be removed because it has rentals.");
        public static readonly DomainError InvalidData = new ("All bike information is required.");
    }

    public static class Rental
    {
        public static readonly DomainError StartDateInThePast = new("Start date cannot be in the past.");
        public static readonly DomainError ReturnBeforeStart = new("Return date cannot be before start date.");
        public static readonly DomainError BikeAlreadyRented = new( "Bike is already rented in the selected period.");
        public static readonly DomainError NotFound = new("Rental not found.");
        public static readonly DomainError BadRequest = new("Rental not found.");
        public static readonly DomainError InvalidData = new("All rental information is required.");
    }

    public static class Storage
    {
        public static readonly DomainError UploadDirectoryUnavailable = new("Uploads directory could not be created or accessed.");
        public static readonly DomainError UploadPermissionDenied = new("There is no permission to save files in the uploads folder.");
        public static readonly DomainError UploadDirectoryNotFound = new("Upload folder not found.");
        public static readonly DomainError UploadIoFailure = new("Error saving file to server.");
        public static readonly DomainError UploadUnexpected = new("Unexpected error while saving file.");
    }
}
