flowchart BT
BasicOwnership(BasicOwnership)
CAFProjectIEncoding(CAFProjectIEncoding)
CAFSingleProjectIEncoding(CAFSingleProjectIEncoding)
ToyTacticsIEncoding(ToyTacticsIEncoding)
XMLEncoding(XMLEncoding)
XMLEncodingStructural(XMLEncodingStructural)
XMLOwnership(XMLOwnership)
_IEncodingGenerator(_IEncodingGenerator)
_IOwnershipGenerator(_IOwnershipGenerator)

BasicOwnership  -..->  _IOwnershipGenerator 
CAFProjectIEncoding  -->  CAFSingleProjectIEncoding 
CAFProjectIEncoding  -..->  _IEncodingGenerator 
CAFProjectIEncoding  -->  _IEncodingGenerator 
CAFSingleProjectIEncoding  -..->  _IEncodingGenerator 
ToyTacticsIEncoding  -..->  _IEncodingGenerator 
XMLEncoding  -..->  _IEncodingGenerator 
XMLEncodingStructural  -..->  _IEncodingGenerator 
XMLOwnership  -..->  _IOwnershipGenerator 
