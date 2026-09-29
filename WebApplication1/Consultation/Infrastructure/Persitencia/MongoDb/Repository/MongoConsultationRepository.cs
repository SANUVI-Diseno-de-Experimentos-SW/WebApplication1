﻿using WebApplication1.Consultation.Domain.Repositories;
using WebApplication1.Consultation.Infrastructure.Mappers;
using WebApplication1.Consultation.Infrastructure.Persitencia.MongoDb.Models;

namespace WebApplication1.Consultation.Infrastructure.Persitencia.MongoDb.Repository;

using Microsoft.Extensions.Logging;
using MongoDB.Driver;


public class MongoConsultationRepository : IConsultationRepository
{
    private readonly IMongoCollection<ConsultationDocument> _collection;
    private readonly ILogger<MongoConsultationRepository> _logger;

    public MongoConsultationRepository(IMongoDatabase database, ILogger<MongoConsultationRepository> logger)
    {
        _collection = database.GetCollection<ConsultationDocument>("consultations");
        _logger = logger;
    }

    public async Task SaveAsync(Domain.Models.Aggregate.Consultation consultation)
    {
        var data = consultation.ToPrimitives();

        var document = new ConsultationDocument
        {
            ConsultationId = data.Id,
            PatientId = data.PatientId,
            MotherId = data.MotherId,
            NurseId = data.NurseId,
            CreatedAt = data.CreatedAt,
            ClosedAt = data.ClosedAt,
            Messages = data.Messages.Select(m => new MessageDocument
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderRole = m.SenderRole,
                Content = m.Content,
                SentAt = m.SentAt
            }).ToList(),
            UpdatedAt = DateTime.UtcNow
        };

        await _collection.InsertOneAsync(document);
        _logger.LogInformation("Consulta creada: {ConsultationId}", document.ConsultationId);
    }

    public async Task UpdateAsync(Domain.Models.Aggregate.Consultation consultation)
    {
        var data = consultation.ToPrimitives();
        var consultationId = data.Id;

        var filter = Builders<ConsultationDocument>.Filter.Eq(x => x.ConsultationId, consultationId);

        var messages = data.Messages.Select(m => new MessageDocument
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderRole = m.SenderRole,
            Content = m.Content,
            SentAt = m.SentAt
        }).ToList();

        var update = Builders<ConsultationDocument>.Update
            .Set(x => x.Messages, messages)
            .Set(x => x.ClosedAt, data.ClosedAt)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update);
        _logger.LogInformation("Consulta actualizada: {ConsultationId}", consultationId);
    }

    public async Task<Domain.Models.Aggregate.Consultation?> FindByIdAsync(string consultationId)
    {
        var filter = Builders<ConsultationDocument>.Filter.Eq(x => x.ConsultationId, consultationId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return ConsultationMapper.ToDomain(document);
    }

    public async Task<List<Domain.Models.Aggregate.Consultation>> FindOpenByMotherIdAsync(string motherId)
    {
        var filter = Builders<ConsultationDocument>.Filter.And(
            Builders<ConsultationDocument>.Filter.Eq(x => x.MotherId, motherId),
            Builders<ConsultationDocument>.Filter.Eq(x => x.ClosedAt, null)
        );
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(ConsultationMapper.ToDomain).ToList();
    }

    public async Task<List<Domain.Models.Aggregate.Consultation>> FindOpenByNurseIdAsync(string nurseId)
    {
        var filter = Builders<ConsultationDocument>.Filter.And(
            Builders<ConsultationDocument>.Filter.Eq(x => x.NurseId, nurseId),
            Builders<ConsultationDocument>.Filter.Eq(x => x.ClosedAt, null)
        );
        var documents = await _collection.Find(filter).ToListAsync();

        return documents.Select(ConsultationMapper.ToDomain).ToList();
    }

    public async Task<Domain.Models.Aggregate.Consultation?> FindOpenByPatientIdAsync(string patientId)
    {
        var filter = Builders<ConsultationDocument>.Filter.And(
            Builders<ConsultationDocument>.Filter.Eq(x => x.PatientId, patientId),
            Builders<ConsultationDocument>.Filter.Eq(x => x.ClosedAt, null)
        );
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document == null) return null;

        return ConsultationMapper.ToDomain(document);
    }

    public async Task DeleteAsync(string consultationId)
    {
        var filter = Builders<ConsultationDocument>.Filter.Eq(x => x.ConsultationId, consultationId);
        await _collection.DeleteOneAsync(filter);
        _logger.LogInformation("Consulta eliminada: {ConsultationId}", consultationId);
    }
}