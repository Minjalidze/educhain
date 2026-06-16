import { expect } from "chai";
import { ethers } from "hardhat";
import { anyValue } from "@nomicfoundation/hardhat-chai-matchers/withArgs";

describe("DocumentVerification", function () {
  const hashA = ethers.keccak256(ethers.toUtf8Bytes("diploma-a"));
  const hashB = ethers.keccak256(ethers.toUtf8Bytes("diploma-b"));

  async function deployFixture() {
    const [owner, issuer, verifier] = await ethers.getSigners();
    const factory = await ethers.getContractFactory("DocumentVerification");
    const contract = await factory.deploy();
    await contract.waitForDeployment();
    return { contract, owner, issuer, verifier };
  }

  it("allows owner to add issuer and issuer to add document", async function () {
    const { contract, issuer } = await deployFixture();

    await expect(contract.addIssuer(issuer.address))
      .to.emit(contract, "IssuerAdded")
      .withArgs(issuer.address);

    await expect(contract.connect(issuer).addDocument(hashA))
      .to.emit(contract, "DocumentAdded")
      .withArgs(hashA, issuer.address, anyValue);

    const result = await contract.verifyDocument(hashA);
    expect(result.exists).to.equal(true);
    expect(result.revoked).to.equal(false);
    expect(result.issuer).to.equal(issuer.address);
  });

  it("rejects duplicate document hash", async function () {
    const { contract } = await deployFixture();

    await contract.addDocument(hashA);

    await expect(contract.addDocument(hashA))
      .to.be.revertedWithCustomError(contract, "DocumentAlreadyExists");
  });

  it("rejects non issuer", async function () {
    const { contract, verifier } = await deployFixture();

    await expect(contract.connect(verifier).addDocument(hashB))
      .to.be.revertedWithCustomError(contract, "OnlyIssuer");
  });

  it("allows issuer to revoke own document", async function () {
    const { contract, issuer } = await deployFixture();

    await contract.addIssuer(issuer.address);
    await contract.connect(issuer).addDocument(hashA);

    await expect(contract.connect(issuer).revokeDocument(hashA))
      .to.emit(contract, "DocumentRevoked");

    const result = await contract.verifyDocument(hashA);
    expect(result.exists).to.equal(true);
    expect(result.revoked).to.equal(true);
  });

  it("prevents another issuer from revoking a document", async function () {
    const { contract, issuer, verifier } = await deployFixture();

    await contract.addIssuer(issuer.address);
    await contract.addIssuer(verifier.address);
    await contract.connect(issuer).addDocument(hashA);

    await expect(contract.connect(verifier).revokeDocument(hashA))
      .to.be.revertedWithCustomError(contract, "RevokeForbidden");
  });
});
