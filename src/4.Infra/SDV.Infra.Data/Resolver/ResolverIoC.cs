using Microsoft.Extensions.DependencyInjection;
using SDV.Domain.Interfaces.Agendas;
using SDV.Domain.Interfaces.Calendars;
using SDV.Domain.Interfaces.Clients;
using SDV.Domain.Interfaces.Messages;
using SDV.Domain.Interfaces.Payments;
using SDV.Domain.Interfaces.Planners;
using SDV.Domain.Interfaces.Plans;
using SDV.Domain.Interfaces.Orders;
using SDV.Infra.Data.MongoDB;
using SDV.Infra.Data.Repository.Agendas;
using SDV.Infra.Data.Repository.Calendars;
using SDV.Infra.Data.Repository.Clients;
using SDV.Infra.Data.Repository.Messages;
using SDV.Infra.Data.Repository.Payments;
using SDV.Infra.Data.Repository.Planners;
using SDV.Infra.Data.Repository.Plans;
using SDV.Infra.Data.Repository.Orders;
using SDV.Infra.Data.Service.Agendas;
using SDV.Infra.Data.Service.Calendars;
using SDV.Infra.Data.Service.Clients;
using SDV.Infra.Data.Service.Messages;
using SDV.Infra.Data.Service.Planners;
using SDV.Infra.Data.Service.Plans;
using SDV.Infra.Data.Service.Orders;
using SDV.Infra.File;
using SDV.Infra.Data.Service.Payments;

namespace SDV.Infra.Data.Resolver;

public static class ResolverIoC
{
    public static IServiceCollection AddDataRepository(this IServiceCollection services)
    {
        //MongoDB
        MongoDbConfig.Configure();
        
        // Cliente
        services.AddTransient<IClientRepository, ClientRepository>();
        services.AddTransient<IClientService, ClientService>();
        //Agenda
        services.AddTransient<IAgendaRepository, AgendaRepository>();
        services.AddTransient<IAgendaService, AgendaService>();
        //Planner
        services.AddTransient<IPlannerRepository, PlannerRepository>();
        services.AddTransient<IPlannerService, PlannerService>();
        //Calendar
        services.AddTransient<ICalendarRepository, CalendarRepository>();
        services.AddTransient<ICalendarService, CalendarService>();
        //Message
        services.AddTransient<IMessageRepository, MessageRepository>();
        services.AddTransient<IMessageService, MessageService>();
        //Order
        services.AddTransient<IOrderRepository, OrderRepository>();
        services.AddTransient<IOrderService, OrderService>();
        //Plan
        services.AddTransient<IPlanRepository, PlanRepository>();
        services.AddTransient<IPlanService, PlanService>();
        //Payment
        services.AddTransient<IPaymentRepository, PaymentRepository>();
        services.AddTransient<IPaymentService, PaymentService>();
        
        // Agenda File Generator        
        services.AddTransient<IAgendaFileGeneratorService>(sp =>
        {
            var calendarService = sp.GetRequiredService<ICalendarService>();
            var messageService = sp.GetRequiredService<IMessageService>();

            return new AgendaFileGeneratorService(calendarService, messageService);
        }); 

        // Planner File Generator        
        services.AddTransient<IPlannerFileGeneratorService>(sp =>
        {
            var calendarService = sp.GetRequiredService<ICalendarService>();
            var messageService = sp.GetRequiredService<IMessageService>();

            return new PlannerFileGeneratorService(calendarService, messageService);
        }); 
        
        return services;
    }

}
