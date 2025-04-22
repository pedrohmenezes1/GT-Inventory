const User = require('../models/userModel');
const bcrypt = require('bcryptjs');
const jwt = require('jsonwebtoken');

// Registrar um novo usuário
exports.register = async (req, res) => {
    const { username, password } = req.body;

    try {
        const hashedPassword = await bcrypt.hash(password, 10);
        const newUser  = new User({ username, password: hashedPassword });
        await newUser .save();
        res.status(201).json({ message: 'Usuário criado com sucesso!' });
    } catch (error) {
        res.status(500).json({ error: 'Erro ao