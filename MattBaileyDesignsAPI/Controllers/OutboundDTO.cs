namespace MattBaileyDesignsAPI.Controllers
{
    sealed public class OutboundDTO
    {
        private readonly Dictionary<string, object> _dtoValue;

        public OutboundDTO(Dictionary<string, object> dtoValue)
        {
            _dtoValue = dtoValue;
        }

        public void AddData(KeyValuePair<string, object> val) { 
            _dtoValue.Add(val.Key, val.Value);
        }

        public Dictionary<string, object> currentDtoValue { 
            get 
            {
                return _dtoValue;    
            } 
        }
    }
}
