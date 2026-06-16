// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

contract DocumentVerification {
    struct DocumentRecord {
        address issuer;
        uint256 issuedAt;
        bool exists;
        bool revoked;
    }

    address public owner;

    mapping(address => bool) public issuers;
    mapping(bytes32 => DocumentRecord) private documents;

    event IssuerAdded(address indexed issuer);
    event IssuerRemoved(address indexed issuer);
    event DocumentAdded(bytes32 indexed documentHash, address indexed issuer, uint256 issuedAt);
    event DocumentRevoked(bytes32 indexed documentHash, address indexed issuer, uint256 revokedAt);

    error OnlyOwner();
    error OnlyIssuer();
    error EmptyDocumentHash();
    error DocumentAlreadyExists();
    error DocumentNotFound();
    error RevokeForbidden();
    error ZeroAddress();

    constructor() {
        owner = msg.sender;
        issuers[msg.sender] = true;
        emit IssuerAdded(msg.sender);
    }

    modifier onlyOwner() {
        if (msg.sender != owner) {
            revert OnlyOwner();
        }
        _;
    }

    modifier onlyIssuer() {
        if (!issuers[msg.sender]) {
            revert OnlyIssuer();
        }
        _;
    }

    function addIssuer(address issuer) external onlyOwner {
        if (issuer == address(0)) {
            revert ZeroAddress();
        }

        issuers[issuer] = true;
        emit IssuerAdded(issuer);
    }

    function removeIssuer(address issuer) external onlyOwner {
        if (issuer == address(0)) {
            revert ZeroAddress();
        }

        issuers[issuer] = false;
        emit IssuerRemoved(issuer);
    }

    function addDocument(bytes32 documentHash) external onlyIssuer {
        if (documentHash == bytes32(0)) {
            revert EmptyDocumentHash();
        }

        if (documents[documentHash].exists) {
            revert DocumentAlreadyExists();
        }

        documents[documentHash] = DocumentRecord({
            issuer: msg.sender,
            issuedAt: block.timestamp,
            exists: true,
            revoked: false
        });

        emit DocumentAdded(documentHash, msg.sender, block.timestamp);
    }

    function verifyDocument(bytes32 documentHash)
        external
        view
        returns (bool exists, bool revoked, address issuer, uint256 issuedAt)
    {
        DocumentRecord memory record = documents[documentHash];
        return (record.exists, record.revoked, record.issuer, record.issuedAt);
    }

    function revokeDocument(bytes32 documentHash) external {
        DocumentRecord storage record = documents[documentHash];

        if (!record.exists) {
            revert DocumentNotFound();
        }

        if (record.issuer != msg.sender && msg.sender != owner) {
            revert RevokeForbidden();
        }

        record.revoked = true;
        emit DocumentRevoked(documentHash, msg.sender, block.timestamp);
    }
}
