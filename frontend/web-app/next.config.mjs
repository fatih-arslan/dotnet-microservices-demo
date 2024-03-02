/** @type {import('next').NextConfig} */
const nextConfig = {   
    images:{
        remotePatterns: [
            {
            hostname:'cdn.pixabay.com'            
            }            
        ]
    },    
};

export default nextConfig;
