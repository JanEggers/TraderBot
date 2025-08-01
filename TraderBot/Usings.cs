global using System;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using MediatR;

global using AlphaVantage;
global using AlphaVantage.Stocks;

global using TraderBot;
global using TraderBot.Contracts;
global using TraderBot.Extensions;
global using TraderBot.Strategies;
global using TraderBot.Models;
global using TraderBot.Requests;



