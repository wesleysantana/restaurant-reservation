using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Moq;
using RestaurantReservation.Application.DTOs.Request.Table;
using RestaurantReservation.Application.DTOs.Response.Table;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using RestaurantReservation.Domain.Repositories;
using RestaurantReservation.Domain.ValueObjects;
using Xunit;

namespace RestaurantReservation.Test.Application
{
    public class TableAppServiceTests
    {
        private readonly Mock<ITableRepository> _tableRepositoryMock;
        private readonly TableAppService _service;

        public TableAppServiceTests()
        {
            _tableRepositoryMock = new Mock<ITableRepository>();
            _service = new TableAppService(_tableRepositoryMock.Object);
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedDtos_WhenTablesExist()
        {
            // Arrange
            var table1 = new Table(
                new Name("Mesa 01"),
                new Capacity(4),
                StatusTable.Disponivel);

            var table2 = new Table(
                new Name("Mesa 02"),
                new Capacity(2),
                StatusTable.Reservada);

            var tables = new List<Table> { table1, table2 }.AsReadOnly();

            _tableRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(tables);

            // Act
            var result = await _service.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var dtos = result.Value;

            Assert.Equal(2, dtos.Count);
            Assert.Collection(dtos,
                dto =>
                {
                    Assert.Equal(table1.Id, dto.Id);
                    Assert.Equal("Mesa 01", dto.Name);
                    Assert.Equal(4, dto.Capacity);
                    Assert.Equal(StatusTable.Disponivel, dto.Status);
                },
                dto =>
                {
                    Assert.Equal(table2.Id, dto.Id);
                    Assert.Equal("Mesa 02", dto.Name);
                    Assert.Equal(2, dto.Capacity);
                    Assert.Equal(StatusTable.Reservada, dto.Status);
                });

            _tableRepositoryMock.Verify(
                r => r.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldFail_WhenTableDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _tableRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Table?)null);

            // Act
            var result = await _service.GetByIdAsync(id, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Table not found.", error.Message);

            _tableRepositoryMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenTableExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            var table = new Table(
                new Name("Mesa 01"),
                new Capacity(4),
                StatusTable.Disponivel);
          
            _tableRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(table);

            // Act
            var result = await _service.GetByIdAsync(id, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var dto = result.Value;

            Assert.Equal(table.Id, dto.Id);
            Assert.Equal("Mesa 01", dto.Name);
            Assert.Equal(4, dto.Capacity);
            Assert.Equal(StatusTable.Disponivel, dto.Status);

            _tableRepositoryMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);
        }
       
        [Fact]
        public async Task CreateAsync_ShouldCreateTable_WithDisponivelStatus_AndReturnDto()
        {
            // Arrange
            var request = new CreateTableRequest
            {
                Name = "Mesa Nova",
                Capacity = 6
            };

            Table? capturedTable = null;

            _tableRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>()))
                .Callback<Table, CancellationToken>((t, _) => capturedTable = t)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var dto = result.Value;

            Assert.NotNull(capturedTable);
            Assert.Equal(capturedTable!.Id, dto.Id);
            Assert.Equal("Mesa Nova", dto.Name);
            Assert.Equal(6, dto.Capacity);
            Assert.Equal(StatusTable.Disponivel, dto.Status);

            Assert.Equal(StatusTable.Disponivel, capturedTable.Status);
            Assert.Equal("Mesa Nova", capturedTable.Name.Value);
            Assert.Equal(6, capturedTable.Capacity.Value);

            _tableRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
       
        [Fact]
        public async Task UpdateAsync_ShouldFail_WhenTableDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            var request = new UpdateTableRequest
            {
                Id = id,
                Name = "Mesa Atualizada",
                Capacity = 8,
                Status = StatusTable.Reservada
            };

            _tableRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Table?)null);

            // Act
            var result = await _service.UpdateAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Table not found.", error.Message);

            _tableRepositoryMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);

            _tableRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTable_WhenTableExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            var table = new Table(
                new Name("Mesa Antiga"),
                new Capacity(4),
                StatusTable.Disponivel);

            var request = new UpdateTableRequest
            {
                Id = id,
                Name = "Mesa Atualizada",
                Capacity = 8,
                Status = StatusTable.Reservada
            };

            _tableRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(table);

            _tableRepositoryMock
                .Setup(r => r.UpdateAsync(table, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _tableRepositoryMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);

            _tableRepositoryMock.Verify(
                r => r.UpdateAsync(table, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldFail_WhenTableDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _tableRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Table?)null);

            // Act
            var result = await _service.DeleteAsync(id, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Table not found.", error.Message);

            _tableRepositoryMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);

            _tableRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<Table>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTable_WhenTableExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            var table = new Table(
                new Name("Mesa 01"),
                new Capacity(4),
                StatusTable.Disponivel);

            _tableRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(table);

            _tableRepositoryMock
                .Setup(r => r.DeleteAsync(table, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(id, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            _tableRepositoryMock.Verify(
                r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
                Times.Once);

            _tableRepositoryMock.Verify(
                r => r.DeleteAsync(table, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
