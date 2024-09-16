import axios from 'axios';

const API_URL = 'https://localhost:7147/api/Users'; // insert api!!!!!!!!!!!!!

export const login = async (email: string, password: string) => {
    try {
        const response = await axios.post(`${API_URL}/login`, { email, password });
        return response.data; 
    } catch (error) {
        if (error instanceof Error) {
            throw new Error(error.message || 'An error occurred during login');
        } else {
            throw new Error('An unknown error occurred');
        }
    }
};