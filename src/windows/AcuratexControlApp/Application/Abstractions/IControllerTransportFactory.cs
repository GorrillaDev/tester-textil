using AcuratexControlApp.Application.Models;

namespace AcuratexControlApp.Application.Abstractions;

public interface IControllerTransportFactory
{
    IControllerTransport Create(ConnectionRequest request);
}
