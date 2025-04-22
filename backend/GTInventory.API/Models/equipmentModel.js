const mongoose = require('mongoose');

const equipmentSchema = new mongoose.Schema({
    name: { type: String, required: true },
    type: { type: String, required: true },
    serialNumber: { type: String, required: true, unique: true },
    location: { type: String, required: true },
    status: { type: String, default: 'available' }, // disponível, em uso, etc.
});

module.exports = mongoose.model('Equipment', equipmentSchema);