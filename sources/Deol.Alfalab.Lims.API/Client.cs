﻿using Deol.Alfalab.Lims.API.Messages;
using Deol.Alfalab.Lims.API.Messages.Base;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Deol.Alfalab.Lims.API
{
    public class Client : IDisposable
    {
        private HttpClient HttpClient { get; } = new HttpClient();

        public void Dispose() => this.HttpClient.Dispose();

        public Client(Uri baseAddress) => this.HttpClient.BaseAddress = baseAddress;

        public Client(string baseAddress) : this(new Uri(baseAddress)) { }

        private async Task<TResponseMessage> SendMessageAsync<TResponseMessage>(IRequestMessage queryMessage)
            where TResponseMessage : IResponseMessage, new()
        {
            return await ParsingResponseAsync<TResponseMessage>(await this.SendRequestAsync(queryMessage));
        }

        private async Task<HttpResponseMessage> SendRequestAsync(IRequestMessage queryMessage)
        {
            try
            {
                var content = new StringContent(queryMessage.ToXMLMessage(), Encoding.UTF8, "application/xml");

                return await this.HttpClient.PostAsync("/", content);
            }
            catch (Exception ex)
            {
                throw new SendRequestException("Ошибка при отправке сообщения в ЛИС", ex);
            }
        }

        private async Task<TResponseMessage> ParsingResponseAsync<TResponseMessage>(HttpResponseMessage httpResponseMessage)
            where TResponseMessage : IResponseMessage, new()
        {
            try
            {
                var resultBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();

                var resultString = Encoding.GetEncoding("Windows-1251").GetString(resultBytes);

                var response = new TResponseMessage();
                response.InitFromXMLMessage(resultString);

                return response;
            }
            catch (Exception ex)
            {
                throw new ParsingResponseExсeption("Оибка при разборе ответа от ЛИС", ex);
            }
        }

        public async Task<ResponseBlankFile> GetBlankFileBinaryAsync(RequestBlankFile request)
        {

            return await ParsingResponseFileBinaryAsync(await this.SendRequestAsync(request));

            async Task<ResponseBlankFile> ParsingResponseFileBinaryAsync(HttpResponseMessage httpResponseMessage)
            {
                var resultBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();

                var contentType = httpResponseMessage.Content.Headers.ContentType.MediaType;

                var responseBlankFile = new ResponseBlankFile();

                if (contentType == "application/xml")
                {
                    var resultString = Encoding.GetEncoding("Windows-1251").GetString(resultBytes);
                    responseBlankFile.InitMessage(resultString);
                }
                else if (contentType.Substring(0, 12) == "application/")
                {
                    var fileExtension = contentType.Substring(12);
                    responseBlankFile.InitBinaryData(resultBytes, fileExtension);
                }
                else
                {
                    responseBlankFile.InitBinaryData(resultBytes, null);
                }

                return responseBlankFile;
            }
        }

        public async Task<ResponseImportReferral> CreateOrUpdateReferralAsync(RequestCreateOrUpdateReferral request) => await this.SendMessageAsync<ResponseImportReferral>(request);

        public async Task<ResponseImportReferral> RemoveReferralAsync(RequestRemoveReferral request) => await this.SendMessageAsync<ResponseImportReferral>(request);

        public async Task<ResponseCreateDoctorOrders> CreateDoctorOrdersAsync(RequestCreateDoctorOrders request) => await this.SendMessageAsync<ResponseCreateDoctorOrders>(request);

        public async Task<ResponseChangeEmail> ChangeEmailAsync(RequestChangeEmail request) => await this.SendMessageAsync<ResponseChangeEmail>(request);

        public async Task<ResponseReferralResults> GetReferralResultsAsync(RequestReferralResults request) => await this.SendMessageAsync<ResponseReferralResults>(request);

        public async Task<ResponseReferralResults> GetNextReferralResultsAsync(RequestNextReferralResults request) => await this.SendMessageAsync<ResponseReferralResults>(request);

        public async Task<ResponseCountReferralResults> GetCountReferralResultsAsync(RequestCountReferralResults request) => await this.SendMessageAsync<ResponseCountReferralResults>(request);

        public async Task<ResponseNewReferralResults> GetNewReferralResultsAsync(RequestNewReferralResults request) => await this.SendMessageAsync<ResponseNewReferralResults>(request);

        public async Task<ResponseMessage> SetReferralResultsImportAsync(RequestReferralResultsImport request) => await this.SendMessageAsync<ResponseMessage>(request);

        public async Task<ResponsePatientReferralResults> GetPatientReferralResultsAsync(RequestPatientReferralResults request) => await this.SendMessageAsync<ResponsePatientReferralResults>(request);

        public async Task<ResponseDictionariesVersion> GetDictionariesVersionAsync(RequestDictionariesVersion request) => await this.SendMessageAsync<ResponseDictionariesVersion>(request);

        public async Task<ResponseDictionaries> GetDictionariesAsync(RequestDictionaries request) => await this.SendMessageAsync<ResponseDictionaries>(request);

        public async Task<ResponsePreprintBarcodes> GetPreprintBarcodesAsync(RequestPreprintBarcodes request) => await this.SendMessageAsync<ResponsePreprintBarcodes>(request);
    }
}