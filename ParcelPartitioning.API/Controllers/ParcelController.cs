using Microsoft.AspNetCore.Mvc;
using NHibernate;
using ParcelPartitioning.Data;
using ParcelPartitioning.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParcelPartitioning.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParcelController : ControllerBase
    {
        private readonly ISession _session;

        public ParcelController(ISession session)
        {
            _session = session;
        }

        [HttpGet]
        [Route("GetParcel")]
        public IEnumerable<Parcel> GetParcel(long parcelId)
        {
            return _session.Query<Parcel>().Where(x => x.ParcelId == parcelId).ToList();
        }

        [HttpGet]
        [Route("GetParcelPartitions")]
        public IEnumerable<ParcelPartition> GetParcelPartitions(long parcelId)
        {
            return _session.Query<ParcelPartition>().Where(x => x.BoxId == parcelId).ToList();
        }

        [HttpPut]
        [Route("AddParcel")]
        public ResponseModel AddParcel([FromBody] ParcelModel request)
        {
            return RunTransaction(AddParcelWorkload, request);
        }

        [HttpPost]
        [Route("UpdateParcel")]
        public ResponseModel UpdateParcel([FromBody] ParcelModel request)
        {
            ResponseModel response = new ResponseModel()
            {
                Success = false
            };
            if (request.PartCount < 2)
            {
                response.Message += "Part count cannot be smaller than 2.\n";
            }

            var existingParcel = GetExistingParcel(request.ParcelId, false);
            if (existingParcel == null)
            {
                response.Message += "Parcel id not found.\n";
                return response;
            }
            if (request.PartCount > existingParcel.Weight)
            {
                response.Message += "Part count cannot be larger than parcel weight.\n";
            }

            if (response.Message != "")
            {
                return response;
            }

            return RunTransaction(UpdateParcelWorkload, request);
        }

        [HttpDelete]
        [Route("DeleteParcel")]
        public ResponseModel DeleteParcel(long request)
        {
            return RunTransaction(DeleteParcelWorkload, request);
        }

        [HttpPost]
        [Route("SetPartCount")]
        public ResponseModel SetPartCount([FromBody] SetParcelPartCountRequest request)
        {
            ResponseModel response = new ResponseModel()
            {
                Success = false
            };
            if (request.ParcelId <= 0)
            {
                response.Message += "Invalid parcel id.\n";
            }
            if (request.PartCount < 2)
            {
                response.Message += "Part count cannot be smaller than 2.\n";
            }

            var existingParcel = GetExistingParcel(request.ParcelId, false);
            if (existingParcel == null)
            {
                response.Message += "Parcel id not found.\n";
                return response;
            }
            if (request.PartCount > existingParcel.Weight)
            {
                response.Message += "Part count cannot be larger than parcel weight.\n";
            }

            if (response.Message != "")
            {
                return response;
            }

            return RunTransaction(UpdateParcelWorkload, new ParcelModel()
            {
                ParcelId = request.ParcelId,
                PartCount = request.PartCount,
                Weight = existingParcel.Weight
            });
        }

        [HttpGet]
        [Route("GetParcelCostReport")]
        public ParcelCostReportResponse GetParcelCostReport(long parcelId)
        {
            var partitions = _session.Query<ParcelPartition>().Where(x => x.BoxId == parcelId).ToList();
            ParcelCostReportResponse response = new ParcelCostReportResponse();
            response.parcelPartitions = partitions;
            response.totalCost = 0;
            foreach (ParcelPartition item in partitions)
            {
                response.totalCost += item.PartCost;
            }

            return response;
        }

        private ResponseModel RunTransaction<T>(Action<T> workload, T parameter)
        {
            ResponseModel response = new ResponseModel() { Success = false };
            try
            {
                var transaction = _session.BeginTransaction();
                transaction.Begin();
                try
                {
                    workload.Invoke(parameter);
                    transaction.Commit();
                    response.Success = true;
                }
                catch (Exception ex)
                {
                    response.Message += ex.Message + "\n";
                    transaction.Rollback();
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            catch (Exception ex)
            {
                response.Message += ex.Message + "\n";
            }
            return response;
        }
        private void AddParcelWorkload(ParcelModel parcel)
        {
            parcel.ParcelId = 0;
            Parcel toInsert = new Parcel()
            {
                ParcelId = parcel.ParcelId,
                PartCount = parcel.PartCount,
                Weight = parcel.Weight
            };
            _session.Save(toInsert);

            foreach (ParcelPartition part in CalculateParcelPartitions(toInsert))
            {
                _session.Save(part);
            }
        }
        private void UpdateParcelWorkload(ParcelModel parcel)
        {
            if (parcel.ParcelId <= 0)
            {
                throw new Exception("Received invalid parcelId.");
            }
            var existingParcel = GetExistingParcel(parcel.ParcelId);
            if (parcel.PartCount != existingParcel.PartCount)
            {
                foreach (var parcelPartition in existingParcel.ParcelPartition)
                {
                    _session.Delete(parcelPartition);
                }
            }

            existingParcel.PartCount = parcel.PartCount;
            existingParcel.Weight = parcel.Weight;

            foreach (ParcelPartition part in CalculateParcelPartitions(existingParcel))
            {
                _session.Save(part);
            }

            _session.Update(existingParcel);
        }
        private void DeleteParcelWorkload(long parcelId)
        {
            var parcel = GetExistingParcel(parcelId);

            foreach (var parcelPartition in parcel.ParcelPartition)
            {
                _session.Delete(parcelPartition);
            }

            _session.Delete(parcel);
        }
        private List<ParcelPartition> CalculateParcelPartitions(Parcel parcel)
        {
            List<ParcelPartition> ret = new List<ParcelPartition>();
            int baseWeight = (int)Math.Floor(parcel.Weight / (decimal)parcel.PartCount);
            int leftOver = parcel.Weight % parcel.PartCount;
            for (int i = 0; i < parcel.PartCount; i++)
            {
                ParcelPartition curr = new ParcelPartition()
                {
                    BoxId = parcel.ParcelId,
                    PartWeight = baseWeight
                };
                if (leftOver != 0)
                {
                    curr.PartWeight++;
                    leftOver--;
                }
                curr.PartCost = 50 + curr.PartWeight * 7;
                ret.Add(curr);
            }
            return ret;
        }
        private Parcel GetExistingParcel(long parcelId, bool throwsException = true)
        {
            var existingParcel = _session.Query<Parcel>().Where(x => x.ParcelId == parcelId).ToList().FirstOrDefault();
            if (existingParcel == null && throwsException)
            {
                throw new Exception("Parcel Id not found in database.\n");
            }
            return existingParcel;
        }

    }
}
